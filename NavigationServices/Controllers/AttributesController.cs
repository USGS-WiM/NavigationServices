//------------------------------------------------------------------------------
//----- HttpController ---------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WIM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   Handles resources through the HTTP uniform interface.
//
//discussion:   Controllers are objects which handle all interaction with resources. 
//              
//
// 

using Microsoft.AspNetCore.Mvc;
using System;
using NavigationAgent;
using System.Threading.Tasks;
using System.Collections.Generic;
using NavigationAgent.Resources;
using WIM.Resources;
using WIM.Services.Attributes;
using Microsoft.Extensions.Options;
using NavigationAgent.ServiceAgents;
using GeoJSON.Net.Geometry;
using System.Linq;
using NavigationDB;
using GeoJSON.Net.CoordinateReferenceSystem;

namespace NavigationServices.Controllers
{
    [Route("[controller]")]
    [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Attributes/summary.md")]
    public class AttributesController : WIM.Services.Controllers.ControllerBase
    {
        private NetworkSettings settings { get; set; }
        public AttributesController(IOptions<NetworkSettings> NetworkSettings) : base()
        {
            this.settings = NetworkSettings.Value;
        }
        #region METHODS
        [HttpGet(Name = "Attributes Resources")]
        [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Attributes/AttributesResources.md")]
        public async Task<IActionResult> Get([FromQuery]Int32? featureid, [FromQuery]double? x, [FromQuery]double? y)
        {
            //returns list of available Navigations
            try
            {
                List<Dictionary<string, object>> results = null;
                if (!featureid.HasValue && (!x.HasValue && !y.HasValue))
                    return new BadRequestObjectResult("An featureid or location (x,y) must be specified.");

                if (x.HasValue && y.HasValue)
                {
                    var agent = new NLDIServiceAgent(settings.NLDI);

                    featureid = Convert.ToInt32(agent.GetLocalCatchmentAsync(new Point(new Position(y.Value, x.Value)) { CRS = new NamedCRS("EPSG:4326") }).Features.FirstOrDefault().Properties["featureid"]);
                }

                using (var NavDBOps = new NavigationDBOps(settings.DBConnectionString))
                {
                   results = NavDBOps.GetAvailableProperties(new List<int>() {featureid.Value }).ToList();
                }//end using

                return Ok(results);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion
        #region HELPER METHODS
        
        #endregion
    }
}
