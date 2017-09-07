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

namespace NavigationAgent
{
    public interface INavigationAgent
    {
        List<Network> GetNetworks();
        Network GetNetwork(string networkIdentifier);
        object GetNetworkRoute(Network selectednetwork);
    }
    public class NavigationAgent : INavigationAgent
    {
        #region Properties
        public NetworkSettings settings { get; set; }
        private NLDIServiceAgent NLDIagent { get; set; }
        private List<Network> AvailableNetworks { get; set; }
        #endregion
        #region Constructor
        public NavigationAgent(IOptions<NetworkSettings> NetworkSettings)
        {
            NLDIagent = new NLDIServiceAgent(NetworkSettings.Value.NLDI);
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
        public object GetNetworkRoute(Network selectednetwork)
        {

            throw new NotImplementedException();
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
        private bool isValid(NavigationOption option)
        {
            try
            {
                //ensures selected network has all required information to begin
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
