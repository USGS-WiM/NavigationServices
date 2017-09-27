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
    internal class StreamStatsServiceAgent : ServiceAgentBase
    {
        #region Properties
        private Dictionary<string, string> Resources { get; set; }
        #endregion
        #region Constructor
        internal StreamStatsServiceAgent(Resource StreamStatsResource) : base(StreamStatsResource.baseurl)
        {
            this.Resources = StreamStatsResource.resources;
        }
        #endregion
        #region Methods
        internal FeatureCollection GetFDirTraceAsync(Point location, Feature mask = null)
        {
            CRSBase crs = null;

            try
            {
                if (mask.Type != GeoJSON.Net.GeoJSONObjectType.Polygon || mask.Type != GeoJSON.Net.GeoJSONObjectType.MultiPoint) mask = null;
                //crs = location.CRS as CRSBase;

                //var reqInfo = GetRequestInfo(nldiservicetype.e_catchment, new object[] { crs.Properties["name"], location.Coordinates.Latitude, location.Coordinates.Longitude });
                //var requestResult = this.ExecuteAsync<FeatureCollection>(reqInfo).Result;

                // this will need another service that Routes over flow direction raster using catchment as mask to catchment outlet
                List<IPosition> coordinates = new List<IPosition>() { new Position(39.79728099057251,-84.08854544162749),
                                                                      new Position(39.79734693545185,-84.08870100975037),
                                                                      new Position(39.79737166476529,-84.08882439136505),
                                                                      new Position(39.797425244913946,-84.08881366252899),
                                                                      new Position(39.79747882502087,-84.08892095088959),
                                                                      new Position(39.797491189655,-84.08903360366821),
                                                                      new Position(39.797503554286884,-84.08914089202881),
                                                                      new Position(39.79751179737358,-84.0892642736435),
                                                                      new Position(39.79758598510945,-84.08928573131561),
                                                                      new Position(39.79759834972431,-84.08935010433196),
                                                                      new Position(39.79769314503106,-84.08956468105316),
                                                                      new Position(39.79769314503106,-84.08965587615967),
                                                                      new Position(39.79769314503106,-84.08977925777435),
                                                                      new Position(39.79769314503106,-84.08983290195464)
                                                                    };

                var feature = new Feature(new LineString(coordinates));

                var requestResult = new FeatureCollection(new List<Feature>() { feature });
;                return requestResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region HelperMethods
        private RequestInfo GetRequestInfo(nldiservicetype requestType, object[] args = null)
        {
            RequestInfo requestInfo = null;
            try
            {
                requestInfo = new RequestInfo(string.Format(GetResourcrUrl(requestType), args));

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
            e_catchment = 1
        }
    }
}
