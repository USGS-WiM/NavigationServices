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
using System.Collections.Generic;
using System.Text;
using WiM.Utilities.ServiceAgent;
using NavigationAgent.Resources;
using GeoJSON.Net.Geometry;

namespace NavigationAgent.ServiceAgents
{
    internal class NLDIServiceAgent : ServiceAgentBase
    {
        #region Properties
        private Dictionary<string, string> Resources { get; set; }
        #endregion
        #region Constructor
        internal NLDIServiceAgent(Resource nldiResource) : base(nldiResource.baseurl)
        {
            this.Resources = nldiResource.resources;
        }
        #endregion
        #region Methods
        internal Catchment GetLocalCatchment(Point location) {

            throw new NotImplementedException();
        }
        #endregion
        #region HelperMethods
        private RequestInfo GetRequestInfo(nldiservicetype requestType, object[] args=null) {
            RequestInfo requestInfo = null;
            try
            {
                requestInfo = new RequestInfo(string.Format(GetResourcrUrl(requestType),args));
                
                return requestInfo;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private String GetResourcrUrl(nldiservicetype filetype)
        {
            try
            {
                String resulturl = string.Empty;
                switch (filetype)
                {
                    case nldiservicetype.e_catchment:
                        resulturl = this.Resources["catchment"];
                        break;
                   
                }//end switch
                return resulturl;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
        private enum nldiservicetype
        {
            e_catchment =1
        }
    }
}
