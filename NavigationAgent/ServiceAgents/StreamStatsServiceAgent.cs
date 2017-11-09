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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using WiM.Utilities.Resources;

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
        internal Feature GetFDirTraceAsync(Feature location, Feature mask)
        {
            CRSBase crs = null;

            try
            {
                crs = location.CRS as CRSBase;
                var body =  getBody(location, crs.Properties["name"].ToString().Replace("EPSG:", ""), mask);
                
                JObject requestResult = this.ExecuteAsync<JObject>(GetResourcrUrl(streamstatsservicetype.e_fdrtrace),new OverRideUrlEncodedContent(body),methodType.e_POST).Result;
                LineString result = requestResult["results"][0].SelectToken("value.trace").ToObject<LineString>();

                Feature traceFeature = new Feature(result, new Dictionary<string, object>{{ "method", "streamstats flow direction trace" },
                                                                                {"description","traverses over flow direction raster to find downstream routing of neighboring cells" } });
                traceFeature.CRS = location.CRS;

                return traceFeature;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region HelperMethods
        private IEnumerable<KeyValuePair<string, string>> getBody(Feature location, string srid, Feature mask) {
            List<KeyValuePair<string, string>> body = new List<KeyValuePair<string, string>>();            
            body.Add(new KeyValuePair<string, string>("Startpoint",JsonConvert.SerializeObject(location)));
            body.Add(new KeyValuePair<string, string>("srid", srid));
            body.Add(new KeyValuePair<string, string>("mask",JsonConvert.SerializeObject(mask)));
            body.Add(new KeyValuePair<string, string>("f", "pjson"));

            return body;
        }
        private String GetResourcrUrl(streamstatsservicetype filetype)
        {
            try
            {
                String resulturl = string.Empty;
                switch (filetype)
                {
                    case streamstatsservicetype.e_fdrtrace:
                        resulturl = this.Resources["fdrTrace"];
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
        private enum streamstatsservicetype
        {
            e_fdrtrace = 1
        }
    }
}
