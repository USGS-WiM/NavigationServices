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
using System.IO;
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
        private Route RouteConfigureation { get; set; }
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
            if (!isValid(selectednetwork.ID, selectednetwork.Configuration)) return false;
            this.RouteConfigureation = new Route((navigationtype)selectednetwork.ID, selectednetwork.Configuration);
            isRouteInitialized = true;

            return isRouteInitialized;
        }        

        public FeatureCollection GetNetworkRoute()
        {
            try
            {
                switch (RouteConfigureation.Type)
                {
                    case navigationtype.e_flowpath:
                        if (!LoadCatchment()) throw new Exception("Catchment failed to load.");
                        if (!LoadFlowDirectionTrace()) throw new Exception("FlowDirection failed to trace.");
                        if (!LoadNetworkTrace(NavigationOption.directiontype.downstream)) throw new Exception("FlowDirection failed to trace.");
                        break;
                    case navigationtype.e_networkpath:
                        if (!LoadCatchment()) throw new Exception("Catchment failed to load.");
                        if (!LoadFlowDirectionTrace()) throw new Exception("FlowDirection failed to trace.");
                        
                        //**Figure** out how all lines are connected(using distance(or truncating polygon) as extent)
                        //  *-> maybe merge overlapping traces and remove the overlap leaving the parent ends

                        break;
                    case navigationtype.e_networktrace:
                        if (!LoadCatchment()) throw new Exception("Catchment failed to load.");
                        if (!LoadFlowDirectionTrace()) throw new Exception("FlowDirection failed to trace.");
                        if (!LoadNetworkTrace()) throw new Exception("NetworkTrace failed.");
                        break;
                }//end switch

                return RouteConfigureation.FeatureCollection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        #region HELPER METHODS
        private Network GetNetworkByID(String networkIdentifier) {
            return this.AvailableNetworks.FirstOrDefault(n => String.Equals(n.ID.ToString(),networkIdentifier,StringComparison.OrdinalIgnoreCase) 
                            || String.Equals(n.Code.Trim(),networkIdentifier.Trim(),StringComparison.OrdinalIgnoreCase));
        }
        private List<NavigationOption> getNetworkOptions(navigationtype nType) {
            List<NavigationOption> options = new List<NavigationOption>();
            options.Add(NavigationOption.StartPointLocationOption());
            options.Add(NavigationOption.LimitOption());

            switch (nType)
            {
                case navigationtype.e_flowpath:
                    break;
                case navigationtype.e_networkpath:
                    options.Insert(1, NavigationOption.EndPointLocationOption());
                    break;
                case navigationtype.e_networktrace:
                    options.Insert(1,NavigationOption.DirectionOption());
                    options.Insert(2, NavigationOption.QuerySourceOption());
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
                switch ((NavigationOption.navigationoptiontype)option.ID)
                {
                    case NavigationOption.navigationoptiontype.e_startpoint:
                    case NavigationOption.navigationoptiontype.e_endpoint:
                        // is valid geojson point
                        if (JsonConvert.DeserializeObject<Point>(JsonConvert.SerializeObject(option.Value)) != null) return true;
                        break;
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

        private bool LoadCatchment() {            
            try
            {
                var options = this.RouteConfigureation.Configuration;

                Point startpoint = JsonConvert.DeserializeObject<Point>(JsonConvert.SerializeObject(options.Find(s => s.ID == (int)NavigationOption.navigationoptiontype.e_startpoint).Value));
       
                this.RouteConfigureation.FeatureCollection.Features.Add(new Feature(startpoint));

                FeatureCollection localCatchment = this.nldiAgent.GetLocalCatchmentAsync(startpoint);

                if (localCatchment == null) return false;
                var comid = localCatchment.Features.FirstOrDefault().Properties["featureid"].ToString();
                if (string.IsNullOrEmpty(comid)) return false;

                this.RouteConfigureation.ComID = comid;
                this.RouteConfigureation.catchment = localCatchment.Features.FirstOrDefault();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool LoadFlowDirectionTrace() {
            try
            {
                var options = this.RouteConfigureation.Configuration;

                var startpoint = JsonConvert.SerializeObject(options.Find(s => s.ID == (int)NavigationOption.navigationoptiontype.e_startpoint).Value);
                var catchmentMask = this.RouteConfigureation.catchment;

                FeatureCollection fldrTrace = this.ssAgent.GetFDirTraceAsync(JsonConvert.DeserializeObject<Point>(startpoint),catchmentMask);
                if (fldrTrace == null) return false;

                this.RouteConfigureation.FeatureCollection.Features.Add(fldrTrace.Features.FirstOrDefault());

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool LoadNetworkTrace(NavigationOption.directiontype tracetype = NavigationOption.directiontype.downstream, NavigationOption.querysourcetype source = NavigationOption.querysourcetype.flowline)
        {
            FeatureCollection traceFC = this.nldiAgent.GetNavigateAsync(this.RouteConfigureation.ComID);

            if (traceFC == null) return false;
            this.RouteConfigureation.FeatureCollection.Features.AddRange(traceFC.Features.Where(f=>!String.Equals(f.Properties["nhdplus_comid"].ToString(),this.RouteConfigureation.ComID)));
            return true;
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
        
        #endregion
    }
}
