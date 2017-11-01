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
using GeoJSON.Net.Feature;
using GeoJSON.Net.CoordinateReferenceSystem;
using System.Linq;
using System.Collections;

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
        internal FeatureCollection GetLocalCatchmentAsync(Point location) {
            CRSBase crs = null;

            try
            {
                crs = location.CRS as CRSBase;

                var reqInfo = GetRequestInfo(nldiservicetype.e_catchment, new object[] { crs.Properties["name"], location.Coordinates.Longitude, location.Coordinates.Latitude });
                var requestResult = this.ExecuteAsync<FeatureCollection>(reqInfo).Result;

                return requestResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        internal FeatureCollection GetNavigateAsync(string startlocation, navigateType navigationMode = navigateType.e_downstream_main, Double? distance =null, querysourceType querysource = querysourceType.e_flowlines)
        {
            try
            {
                var reqInfo = GetRequestInfo(nldiservicetype.e_navigate, new object[] {startlocation,getNavigateMode(navigationMode, querysource), distance?.ToString() ?? ""});
                var requestResult = this.ExecuteAsync<FeatureCollection>(reqInfo).Result;

                return requestResult;
            }
            catch (Exception ex)
            {
                throw;
            }
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
                        resulturl = this.Resources["nhdplusWFS"];
                        break;
                    case nldiservicetype.e_navigate:
                        resulturl = this.Resources["nldiQuery"];
                        break;
                   
                }//end switch
                return resulturl;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        private string getNavigateMode(navigateType ntype, querysourceType qtype)
        {
            string result = "";
            switch (ntype)
            {
                case navigateType.e_downstream_diversions:
                    result = "DD";
                    break;
                case navigateType.e_upstream_main:
                    result = "UM";
                    break;
                case navigateType.e_upstream_tributaries:
                    result = "UT";
                    break;
                case navigateType.e_downstream_main:
                default:
                    result = "DM";
                    break;
            }//end switch
            string qstring = getQuerySource(qtype);
            return string.IsNullOrEmpty(qstring) ? result : result + "/" + qstring;
        }
        private string getQuerySource(querysourceType qtype)
        {
            switch (qtype)
            {
                case querysourceType.e_wqpsites:
                    return "wqp";
                case querysourceType.e_huc12pourpoints:
                    return "huc12pp";
                case querysourceType.e_gage:
                    return "ss_gages";
                default:
                    return "";
            }
        }
        #endregion
        private enum nldiservicetype
        {
            e_catchment =1,
            e_navigate =2
        }
        internal enum navigateType
        {            
            e_downstream_main =1,            
            e_upstream_tributaries=2,
            e_downstream_diversions=3,
            e_upstream_main=4
        }
        internal enum querysourceType
        {
            e_flowlines =0,
            e_wqpsites =1,
            e_gage =2,
            e_huc12pourpoints=9
        }
        
    }
}
