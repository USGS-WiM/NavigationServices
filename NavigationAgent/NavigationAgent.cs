//------------------------------------------------------------------------------
//----- ServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating the service call, 
//              capturing the data that's returned and forwarding the data back to 
//              the requestor.
//
//discussion:   This is the main Agent that manages the subserviant service agents for 
//              Navigation. Navigation Agent puts all the pieces together and returns 
//              the results to the requester
//
// 

using System;
using System.Linq;
using System.Collections.Generic;
using NavigationAgent.ServiceAgents;
using NavigationAgent.Resources;
using Microsoft.Extensions.Options;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using WiM.Resources;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using WiM.Utilities.Resources;
using GeoJSON.Net;
using GeoJSON.Net.Converters;
using WiM.Extensions;

namespace NavigationAgent
{
    public interface INavigationAgent:IMessage
    {
        List<Network> GetNetworks();
        Network GetNetwork(string networkIdentifier);
        bool InitializeRoute(Network selectednetwork);       
        FeatureCollection GetNetworkRoute();        
    }

    public class NavigationAgent : INavigationAgent
    {
        #region Properties
        private bool isRouteInitialized = false;
        private NLDIServiceAgent nldiAgent { get; set; }
#warning this is a temperary agent until gages are migrated to production in NLDI
        private NLDIServiceAgent nldiAgent2 { get; set; }
        private StreamStatsServiceAgent ssAgent { get; set; }
        private List<Network> availableNetworks { get; set; }
        private Route route { get; set; }
        public List<Message> Messages { get; private set; }
        private IGeometryObject clipGeometry { get; set; }
        private Double? distanceLimit { get; set; }

        #endregion
        #region Constructor
        public NavigationAgent(IOptions<NetworkSettings> NetworkSettings)
        {
            nldiAgent = new NLDIServiceAgent(NetworkSettings.Value.NLDI);            
            nldiAgent2 = new NLDIServiceAgent(new Resource() { baseurl = "https://cida-test.er.usgs.gov", resources = NetworkSettings.Value.NLDI.resources });
            ssAgent = new StreamStatsServiceAgent(NetworkSettings.Value.StreamStats);
            availableNetworks = NetworkSettings.Value.Networks;
            Messages = new List<Message>();
        }
        #endregion
        #region Methods
        public List<Network> GetNetworks() {
            
            return Enum.GetValues(typeof(navigationtype)).Cast<navigationtype>()
                .Select(t => getNetworkByID(((Int32)t).ToString())).ToList();
        }
        public Network GetNetwork(string networkIdentifier) {
            var selectedNetwork = getNetworkByID(networkIdentifier);
            selectedNetwork.Configuration = getNetworkOptions((navigationtype)selectedNetwork.ID);
            return selectedNetwork;
        }
        public bool InitializeRoute(Network selectednetwork) {
            this.route = new Route((navigationtype)selectednetwork.ID);
            if (!isValid(selectednetwork.ID, selectednetwork.Configuration)) return false;

            route.Configuration = selectednetwork.Configuration;

            isRouteInitialized = true;
            return isRouteInitialized;
        }        
        public FeatureCollection GetNetworkRoute()
        {
            try
            {
                if (!this.isRouteInitialized) return null;
                switch (route.Type)
                {
                    case navigationtype.e_flowpath:
                        loadFlowPath();
                        break;
                    case navigationtype.e_networkpath:
                        loadNetworkPath();
                        break;
                    case navigationtype.e_networktrace:
                        loadNetworkTrace();                     
                        break;
                }//end switch
                
                return new FeatureCollection(route.Features.Where(f=>f.Value.Id == null).Select(x=>x.Value).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region HELPER METHODS
        private void sm(string message, MessageType type = MessageType.info)
        {
            this.Messages.Add(new Message() { msg=message, type = type });
        }
        private void loadFlowPath(routeoptiontype rotype = routeoptiontype.e_start) {
            
            if (!loadCatchment(rotype)) throw new Exception("Catchment failed to load.");
            if (!loadFlowDirectionTrace(rotype)) Console.WriteLine("FlowDirection failed to trace.");
            
            if (!loadNetworkTrace(rotype,distance: distanceLimit)) Console.WriteLine("FlowDirection failed to trace.");
            mergeFlowDirectionNetworkTrace(rotype);

            if (route.Configuration.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_trunkatingpolygon) != null)
            {

            }


        }
        private void loadNetworkPath() {
            loadFlowPath();
            loadFlowPath(routeoptiontype.e_end);
            removeIntersectingFeatures();
        }
        private void loadNetworkTrace() {
            if (!loadCatchment(routeoptiontype.e_start)) throw new Exception("Catchment failed to load.");
            
            if(!Enum.TryParse(route.Configuration.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_navigationdirection).Value, out NavigationOption.directiontype dtype))
                dtype = NavigationOption.directiontype.downstream;

            var queries = ((List<string>)route.Configuration.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_querysource).Value)
                .Select(x=> { Enum.TryParse(x, out NavigationOption.querysourcetype sType); return sType; }).ToList();
            
            foreach (var item in queries)
            {
                if (!loadNetworkTrace(routeoptiontype.e_start, dtype,distanceLimit,item)) sm("Failed to trace network for "+item,MessageType.error);
            }//next query 

            if (dtype == NavigationOption.directiontype.downstream && queries.Contains(NavigationOption.querysourcetype.flowline))
            {
                if (!loadFlowDirectionTrace(routeoptiontype.e_start)) sm("FlowDirection failed to trace.", MessageType.error);
                else mergeFlowDirectionNetworkTrace(routeoptiontype.e_start);
            }
        }

        private Network getNetworkByID(String networkIdentifier) {
            return this.availableNetworks.FirstOrDefault(n => String.Equals(n.ID.ToString(),networkIdentifier,StringComparison.OrdinalIgnoreCase) 
                            || String.Equals(n.Code.Trim(),networkIdentifier.Trim(),StringComparison.OrdinalIgnoreCase));
        }
        private List<NavigationOption> getNetworkOptions(navigationtype nType) {
            sm("Configuring Network options " + nType.ToString());
            List<NavigationOption> options = new List<NavigationOption>();
            options.Add(NavigationOption.StartPointLocationOption());
            

            switch (nType)
            {
                case navigationtype.e_flowpath:
                    options.Add(NavigationOption.LimitOption());
                    break;
                case navigationtype.e_networkpath:
                    options.Insert(1, NavigationOption.EndPointLocationOption());
                    break;
                case navigationtype.e_networktrace:
                    options.Insert(1,NavigationOption.DirectionOption());
                    options.Insert(2, NavigationOption.QuerySourceOption());
                    options.Add(NavigationOption.LimitOption());
                    break;
                default:
                    sm("No navigation options exist " + nType);
                    break;
            }
            return options;
        }

        private bool isValid(int navigationtypeID, List<NavigationOption> options)
        {
            NavigationOption option = null;
            try
            {

                navigationtype ntype = (navigationtype)navigationtypeID;
                switch (ntype)
                {
                    case navigationtype.e_flowpath:
                        // requires valid startpoint
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_startpoint);
                        if (option == null || !isValid(option)) return false;

                        //limit
                        option = options.FirstOrDefault(x => x.ID == 0);
                        if (option != null && !isValid(option)) return false;

                        break;
                    case navigationtype.e_networkpath:
                        // requires valid startpoint and endpoint
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_startpoint);
                        if (option == null || !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_endpoint);
                        if (option == null || !isValid(option)) return false;

                        //limit
                        option = options.FirstOrDefault(x => x.ID == 0);
                        if (option != null && !isValid(option)) return false;

                        break;
                    case navigationtype.e_networktrace:
                        // requires valid startpoint and upstream/dowstream and data query
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_startpoint);
                        if (option == null || !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_navigationdirection);
                        if (option == null || !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_querysource);
                        if (option == null || !isValid(option)) return false;

                        //limit
                        option = options.FirstOrDefault(x => x.ID == 0);
                        if (option != null && !isValid(option)) return false;

                        break;
                    default:
                        break;
                }

                sm("Options are valid");
                return true;
            }
            catch (Exception ex)
            {
                sm("Error occured while validating " + ex.Message, MessageType.error);
                return false;
                throw;
            }
        }
        private bool isValid(NavigationOption option)
        {
            try
            {
                NavigationOption.navigationoptiontype sType = (NavigationOption.navigationoptiontype)option.ID;
                switch (sType)
                {
                    case NavigationOption.navigationoptiontype.e_startpoint:
                    case NavigationOption.navigationoptiontype.e_endpoint:
                        // is valid geojson point
                        Point point = JsonConvert.DeserializeObject<Point>(JsonConvert.SerializeObject(option.Value));
                        if (point == null) break;                        
                        addRouteFeature(getFeatureName((routeoptiontype)option.ID, navigationfeaturetype.e_point), new Feature(point) { CRS = point.CRS });
                        return true;

                    case NavigationOption.navigationoptiontype.e_distance:
                        double result;
                        if (!Double.TryParse(option.Value.ToString(), out result)) break;
                        this.distanceLimit = result;
                        return true;

                    case NavigationOption.navigationoptiontype.e_trunkatingpolygon:
                        // is valid geojson polygon
                        IGeometryObject poly = JsonConvert.DeserializeObject<IGeometryObject>(JsonConvert.SerializeObject(option.Value), new GeometryConverter());
                        if (poly == null) break;
                        this.clipGeometry = poly;
                        return true;

                    case NavigationOption.navigationoptiontype.e_navigationdirection:
                        if (Enum.IsDefined(typeof(NavigationOption.directiontype), option.Value)) return true;
                        break;
                    case NavigationOption.navigationoptiontype.e_querysource:
                        //loop over and ensure all is defined
                        option.Value = ((JArray)option.Value).ToObject<List<string>>();
                        if(((List<string>)option.Value).All(s=> Enum.IsDefined(typeof(NavigationOption.querysourcetype), s))) return true;
                        break;

                    default:// limit
                        var value = JsonConvert.DeserializeObject<NavigationOption>(JsonConvert.SerializeObject(option.Value));
                        if ((value.ID == (int)NavigationOption.navigationoptiontype.e_distance ||
                             value.ID == (int)NavigationOption.navigationoptiontype.e_trunkatingpolygon) &&
                             isValid(value)) return true;
                        break;

                }
                sm(option.Name + " is invalid. Please provide a valid " + option.Name, MessageType.warning);
                return false;
            }
            catch (Exception ex)
            {
                sm("Error occured while validating "+ option.Name +" "+ ex.Message, MessageType.error);
                return false;
            }
        }
        private bool loadCatchment(routeoptiontype rotype) {
            try
            {
                Point startpoint = ((Point)route.Features[getFeatureName(rotype, navigationfeaturetype.e_point)].Geometry);
                FeatureCollection localCatchment = this.nldiAgent.GetLocalCatchmentAsync(startpoint);

                if (localCatchment == null)
                    throw new Exception(String.Format("nldi failed to return catchment. X {0}, Y {1}", startpoint.Coordinates.Longitude, startpoint.Coordinates.Latitude));
                var comid = localCatchment.Features.FirstOrDefault().Properties["featureid"].ToString();
                if (string.IsNullOrEmpty(comid))
                    throw new Exception(String.Format("nldi catchment does not contain property featureid X {0}, Y {1}", startpoint.Coordinates.Longitude, startpoint.Coordinates.Latitude));
                this.route.ComID = comid;
                addRouteFeature(getFeatureName(rotype,navigationfeaturetype.e_catchment), localCatchment.Features.FirstOrDefault());
                sm("Catchement valid, ComID: " + comid);
                return true;
            }
            catch (Exception ex)
            {
                sm("Catchement invalid " +ex.Message, MessageType.error);
                return false;
            }
        }
        private bool loadFlowDirectionTrace(routeoptiontype rotype) {
            try
            {
                Feature startpoint = route.Features[getFeatureName(rotype, navigationfeaturetype.e_point)];
                Feature catchmentMask = route.Features[getFeatureName(rotype, navigationfeaturetype.e_catchment)];

                Feature fldrTrace = this.ssAgent.GetFDirTraceAsync(startpoint, catchmentMask);
                if (fldrTrace == null)
                    throw new Exception("Flow direction trace object null.");

                addRouteFeature(getFeatureName(rotype, navigationfeaturetype.e_fdrroute), fldrTrace);

                return true;
            }
            catch (Exception ex)
            {
                sm("Error loading Flow direction trace " + ex.Message, MessageType.error);
                return false;
            }
        }
        private bool loadNetworkTrace(routeoptiontype rotype,NavigationOption.directiontype tracetype = NavigationOption.directiontype.downstream, double? distance = null, NavigationOption.querysourcetype source = NavigationOption.querysourcetype.flowline)
        {
            try
            {
                List<Feature> traceItems = null;
                string nameprefix = "";
                FeatureCollection traceFC = this.nldiAgent2.GetNavigateAsync(this.route.ComID,(NLDIServiceAgent.navigateType)tracetype, distance,(NLDIServiceAgent.querysourceType) source);

                if (traceFC == null)
                    throw new Exception("Network trace from nldi agent is null. ComID: "+this.route.ComID);
                if (source == NavigationOption.querysourcetype.flowline)
                {
                    traceItems = traceFC.Features.Where(f => !String.Equals(f.Properties["nhdplus_comid"]?.ToString(), this.route.ComID)).ToList();
                    nameprefix = getFeatureName(rotype, navigationfeaturetype.e_traceroute);
                }
                else
                {
                    traceItems = traceFC.Features;
                    nameprefix = source.ToString();
                }
                for (int i = 0; i < traceItems.Count(); i++)
                {
                    addRouteFeature(nameprefix+i, traceItems[i]);
                }//next item
            
                return true;
            }
            catch (Exception ex)
            {
                sm("Error loading Network trace " + ex.Message, MessageType.error);
                return false;
            }
        }
        private void mergeFlowDirectionNetworkTrace(routeoptiontype rotype) {
            IPosition FDir = null;
            IPosition first = null;
            try
            {

                if (!route.Features.ContainsKey(getFeatureName(rotype, navigationfeaturetype.e_fdrroute)) || 
                    !route.Features.ContainsKey(getFeatureName(rotype, navigationfeaturetype.e_traceroute) + "0"))
                    throw new Exception("Either flow direction or network trace is not available.");

                FDir = ((LineString)route.Features[getFeatureName(rotype, navigationfeaturetype.e_fdrroute)].Geometry).Coordinates.Last();
                first = ((LineString)route.Features[getFeatureName(rotype, navigationfeaturetype.e_traceroute)+"0"].Geometry).Coordinates.First();

                LineString merge = new LineString(new List<IPosition>() { FDir, first });
                route.Features.Add(getFeatureName(rotype, navigationfeaturetype.e_connection), new Feature(merge));

            }
            catch (Exception ex)
            {
                sm("Error Merging Flow direction trace with network trace " + ex.Message, MessageType.warning);
                return;
            }           

        }
        private bool removeIntersectingFeatures()
        {
            try
            {
                //remove the 2 networksTraces and replace
                IEnumerable<string> fullMatchingKeys = route.Features.Keys.Where(currentKey => currentKey.Contains(getFeatureName(0, navigationfeaturetype.e_traceroute)));

                var keysToRemove = fullMatchingKeys.Where(k => route.Features.ContainsKey(k))
                    .GroupBy(k => route.Features[k].Properties["nhdplus_comid"]).Where(x => x.Count() > 1).SelectMany(s => s).ToList();

                foreach (var key in keysToRemove)
                {
                    route.Features.Remove(key);
                }//next key

                return true;
            }
            catch (Exception ex)
            {
                sm("Error removing intersecting features " + ex.Message, MessageType.error);
                return false;
            }
        }

        private string getFeatureName(routeoptiontype roType, navigationfeaturetype ftype) {
            string name = "";
            switch (ftype)
            {                
                case navigationfeaturetype.e_catchment:
                    name= "catchment";
                    break;
                case navigationfeaturetype.e_point:
                    name = "point";
                    break;
                case navigationfeaturetype.e_fdrroute:
                    name = "flow_direction_route";
                    break;
                case navigationfeaturetype.e_traceroute:
                    name = "trace_route";
                    break;
                case navigationfeaturetype.e_connection:
                    name = "connection";
                    break;
                case navigationfeaturetype.e_query:
                    name = "query";
                    break;
                default:
                    name = "notspecified";
                    break;
            }//end switch
            return getRouteOptionName(roType) + "_" + name;
        }
        private string getRouteOptionName(routeoptiontype roType)
        {
            switch (roType)
            {
                case  routeoptiontype.e_start: return "start";
                case routeoptiontype.e_end: return "end";
                default: return "";
            }//end switch
        }

        private void addRouteFeature(string name, Feature feature)
        {
            List<GeoJSONObjectType> applicableTypes = new List<GeoJSONObjectType> { GeoJSONObjectType.LineString, GeoJSONObjectType.Point };
            if (clipGeometry != null && applicableTypes.Contains(feature.Geometry.Type))
            {
                switch (feature.Geometry.Type)
                {
                    case GeoJSON.Net.GeoJSONObjectType.Point:
                        var pnt = (Point)feature.Geometry;
                        switch (clipGeometry.Type)
                        {
                            case GeoJSON.Net.GeoJSONObjectType.Polygon:
                                if (!((Polygon)clipGeometry).ContainsPoint(pnt)) return;
                                break;
                            case GeoJSON.Net.GeoJSONObjectType.MultiPolygon:
                                if (!((MultiPolygon)clipGeometry).ContainsPoint(pnt)) return;
                                break;
                        }//end switch

                        break;
                    case GeoJSON.Net.GeoJSONObjectType.LineString:
                        List<Point> lpnt = ((LineString)feature.Geometry).Coordinates.Select(p=>new Point(p)).ToList();
                        switch (clipGeometry.Type)
                        {
                            case GeoJSON.Net.GeoJSONObjectType.Polygon:
                                if (!lpnt.Any(p=>((Polygon)clipGeometry).ContainsPoint(p))) return;
                                break;
                            case GeoJSON.Net.GeoJSONObjectType.MultiPolygon:
                                if (!lpnt.Any(p => ((MultiPolygon)clipGeometry).ContainsPoint(p))) return;
                                break;
                        }//end switch

                        break;
                }//end switch
            } 
            this.route.Features.Add(name, feature);
        }
        #endregion
        #region Enumerations
        public enum navigationtype
        {
            //"Enumerable integer must match networklist ID in appsettings"
            e_flowpath=1,       // raindrop trace downstream to nhd, then downstream to a distance down stream
            e_networkpath = 2,  // network connection between 2 points
            e_networktrace =3   // upstream/downstream trace, find nearest gage to a specified distance etc.
        }      
        public enum navigationfeaturetype
        {
            e_catchment,
            e_point,
            e_fdrroute,
            e_traceroute,
            e_connection,
            e_query
        }
        public enum routeoptiontype
        {
            //must match navigationoptions start/end point
            e_start=1,
            e_end=2
        }

        #endregion
    }
}