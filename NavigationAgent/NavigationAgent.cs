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
//discussion:   delegated hunting and gathering responsibilities.   
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

namespace NavigationAgent
{
    public interface INavigationAgent
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
        public NetworkSettings settings { get; set; }
        private NLDIServiceAgent nldiAgent { get; set; }
        private StreamStatsServiceAgent ssAgent { get; set; }
        private List<Network> AvailableNetworks { get; set; }

        private Route Route { get; set; }

        #endregion
        #region Constructor
        public NavigationAgent(IOptions<NetworkSettings> NetworkSettings)
        {
            nldiAgent = new NLDIServiceAgent(NetworkSettings.Value.NLDI);
            ssAgent = new StreamStatsServiceAgent(NetworkSettings.Value.StreamStats);
            AvailableNetworks = NetworkSettings.Value.Networks;
        }
        #endregion
        #region Methods
        public List<Network> GetNetworks() {

            return Enum.GetValues(typeof(navigationtype)).Cast<navigationtype>()
                .Select(t => GetNetworkByID(((Int32)t).ToString())).ToList();
        }
        public Network GetNetwork(string networkIdentifier) {
            var selectedNetwork = GetNetworkByID(networkIdentifier);
            selectedNetwork.Configuration = getNetworkOptions((navigationtype)selectedNetwork.ID);
            return selectedNetwork;
        }
        public bool InitializeRoute(Network selectednetwork) {
            this.Route = new Route((navigationtype)selectednetwork.ID);
            if (!isValid(selectednetwork.ID, selectednetwork.Configuration)) return false;

            Route.Configuration = selectednetwork.Configuration;

            isRouteInitialized = true;
            return isRouteInitialized;
        }        

        public FeatureCollection GetNetworkRoute()
        {
            try
            {
                if (!this.isRouteInitialized) return null;
                switch (Route.Type)
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
                
                return new FeatureCollection(Route.Features.Where(f=>f.Value.Id == null).Select(x=>x.Value).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region HELPER METHODS
        private void loadFlowPath(routeoptiontype rotype = routeoptiontype.e_start) {
            
            if (!LoadCatchment(rotype)) throw new Exception("Catchment failed to load.");
            if (!LoadFlowDirectionTrace(rotype)) throw new Exception("FlowDirection failed to trace.");
            if (!LoadNetworkTrace(rotype)) throw new Exception("FlowDirection failed to trace.");
            MergeFlowDirectionNetworkTrace(rotype);

        }
        private void loadNetworkPath() {
            loadFlowPath();
            loadFlowPath(routeoptiontype.e_end);
            IntersectAndReplaceNetwork();
        }
        private void loadNetworkTrace() {
            if (!LoadCatchment(routeoptiontype.e_start)) throw new Exception("Catchment failed to load.");

            var directiontype = Route.Configuration.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_navigationdirection);
            var queries = Route.Configuration.Where(x => x.ID == (int)NavigationOption.navigationoptiontype.e_querysource).ToList();

            if(directiontype.Value == NavigationOption.directiontype.downstream)
                if (!LoadFlowDirectionTrace(routeoptiontype.e_start)) throw new Exception("FlowDirection failed to trace.");

            if (!LoadNetworkTrace(routeoptiontype.e_start)) throw new Exception("NetworkTrace failed.");
        }

        private Network GetNetworkByID(String networkIdentifier) {
            return this.AvailableNetworks.FirstOrDefault(n => String.Equals(n.ID.ToString(),networkIdentifier,StringComparison.OrdinalIgnoreCase) 
                            || String.Equals(n.Code.Trim(),networkIdentifier.Trim(),StringComparison.OrdinalIgnoreCase));
        }
        private List<NavigationOption> getNetworkOptions(navigationtype nType) {
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

                        //optional
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_distance);
                        if (option != null && !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_trunkatingpolygon);
                        if (option != null && !isValid(option)) return false;

                        break;
                    case navigationtype.e_networkpath:
                        // requires valid startpoint and endpoint
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_startpoint);
                        if (option == null || !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_endpoint);
                        if (option == null || !isValid(option)) return false;

                        //optional
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_distance);
                        if (option != null && !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_trunkatingpolygon);
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

                        //optional
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_distance);
                        if (option != null && !isValid(option)) return false;
                        option = options.FirstOrDefault(x => x.ID == (int)NavigationOption.navigationoptiontype.e_trunkatingpolygon);
                        if (option != null && !isValid(option)) return false;
                        break;
                    default:
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
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
                        if (point == null) return false;                        
                        this.Route.Features.Add(getFeatureName((routeoptiontype)option.ID, navigationfeaturetype.e_point), new Feature(point) { CRS = point.CRS });
                        return true;

                    case NavigationOption.navigationoptiontype.e_distance:
                        double result;
                        if (Double.TryParse(option.Value.ToString(), out result)) return true;
                        break;
                    case NavigationOption.navigationoptiontype.e_trunkatingpolygon:
                        // is valid geojson polygon  
                        if (JsonConvert.DeserializeObject<Polygon>(JsonConvert.SerializeObject(option.Value)) != null ||
                            JsonConvert.DeserializeObject<MultiPolygon>(JsonConvert.SerializeObject(option.Value)) != null) return true;
                        break;
                    case NavigationOption.navigationoptiontype.e_navigationdirection:
                        if (Enum.IsDefined(typeof(NavigationOption.directiontype), option.Value)) return true;
                        break;
                    case NavigationOption.navigationoptiontype.e_querysource:
                        if (Enum.IsDefined(typeof(NavigationOption.querysourcetype), option.Value)) return true;
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool LoadCatchment(routeoptiontype rotype) {
            try
            {
                Point startpoint = ((Point)Route.Features[getFeatureName(rotype, navigationfeaturetype.e_point)].Geometry);
                FeatureCollection localCatchment = this.nldiAgent.GetLocalCatchmentAsync(startpoint);

                if (localCatchment == null) return false;
                var comid = localCatchment.Features.FirstOrDefault().Properties["featureid"].ToString();
                if (string.IsNullOrEmpty(comid)) return false;

                this.Route.ComID = comid;
                this.Route.Features.Add(getFeatureName(rotype,navigationfeaturetype.e_catchment), localCatchment.Features.FirstOrDefault());

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool LoadFlowDirectionTrace(routeoptiontype rotype) {
            try
            {

                Feature startpoint = Route.Features[getFeatureName(rotype, navigationfeaturetype.e_point)];
                Feature catchmentMask = Route.Features[getFeatureName(rotype, navigationfeaturetype.e_catchment)];

                Feature fldrTrace = this.ssAgent.GetFDirTraceAsync(startpoint, catchmentMask);
                if (fldrTrace == null) return false;

                this.Route.Features.Add(getFeatureName(rotype, navigationfeaturetype.e_fdrroute), fldrTrace);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool LoadNetworkTrace(routeoptiontype rotype,NavigationOption.directiontype tracetype = NavigationOption.directiontype.downstream, NavigationOption.querysourcetype source = NavigationOption.querysourcetype.flowline)
        {
            FeatureCollection traceFC = this.nldiAgent.GetNavigateAsync(this.Route.ComID);

            if (traceFC == null) return false;

            var items = traceFC.Features.Where(f => !String.Equals(f.Properties["nhdplus_comid"].ToString(), this.Route.ComID)).ToList();
            for (int i = 0; i < items.Count(); i++)
            {
                this.Route.Features.Add(getFeatureName(rotype, navigationfeaturetype.e_traceroute)+i, items[i]);
            }//next item
            
            return true;
        }
        private void MergeFlowDirectionNetworkTrace(routeoptiontype rotype) {
            try
            {
                IPosition FDir = ((LineString)Route.Features[getFeatureName(rotype, navigationfeaturetype.e_fdrroute)].Geometry).Coordinates.Last();
                IPosition first = ((LineString)Route.Features[getFeatureName(rotype, navigationfeaturetype.e_traceroute)+"0"].Geometry).Coordinates.First();

                LineString merge = new LineString(new List<IPosition>() { FDir, first });
                Route.Features.Add(getFeatureName(rotype, navigationfeaturetype.e_connection), new Feature(merge));
            }
            catch (Exception)
            {
                
            }
           

        }
        private bool IntersectAndReplaceNetwork()
        {
            try
            {
                //remove the 2 networksTraces and replace
                IEnumerable<string> fullMatchingKeys = Route.Features.Keys.Where(currentKey => currentKey.Contains(getFeatureName(0, navigationfeaturetype.e_traceroute)));

                var keysToRemove = fullMatchingKeys.Where(k => Route.Features.ContainsKey(k))
                    .GroupBy(k => Route.Features[k].Properties["nhdplus_comid"]).Where(x => x.Count() > 1).SelectMany(s => s).ToList();

                foreach (var key in keysToRemove)
                {
                    Route.Features.Remove(key);
                }//next key

                return true;
            }
            catch (Exception)
            {
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
            return getRoutOptionName(roType) + "_" + name;
        }
        private string getRoutOptionName(routeoptiontype roType)
        {
            switch (roType)
            {
                case  routeoptiontype.e_start: return "start";
                case routeoptiontype.e_end: return "end";
                default: return "";
            }//end switch
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
