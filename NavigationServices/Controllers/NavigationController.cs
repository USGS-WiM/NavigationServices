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

namespace NavigationServices.Controllers
{
    [Route("[controller]")]
    [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Navigation/summary.md")]
    public class NavigationController : WIM.Services.Controllers.ControllerBase
    {
        public INavigationAgent agent { get; set; }
        public NavigationController(INavigationAgent agent ) : base()
        {
            this.agent = agent;
        }
        #region METHODS
        [HttpGet(Name = "Available Navigation Resources")]
        [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Navigation/AvailableNavigationResources.md")]
        public async Task<IActionResult> Get()
        {
            //returns list of available Navigations
            try
            {
                return Ok(agent.GetNetworks());
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpGet("{CodeOrID}", Name = "Navigation Resource")]
        [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Navigation/NavigationResource.md")]
        public async Task<IActionResult> Get(string CodeOrID)
        {
            //returns list of available Navigations
            try
            {                
                var selectednetwork = agent.GetNetwork(CodeOrID);
                if (selectednetwork == null) return new BadRequestObjectResult("Network not found.");
                
                return Ok(selectednetwork);
            }
            catch (Exception ex)
            {
                
                return HandleException(ex);
            }
        }

        [Route("{CodeOrID}/Route", Name = "Navigation Resource Feature Route")]
        [APIDescription(type = DescriptionType.e_link, Description = "/Docs/Navigation/NavigationResourceFeatureRoute.md")]
        public async Task<IActionResult> Route(string CodeOrID, [FromBody]List<NavigationOption> options, [FromQuery] bool properties=false)
        {
            try
            {
                agent.IncludeVAA = properties;
                var selectednetwork = agent.GetNetwork(CodeOrID);
                if (selectednetwork == null) return new BadRequestObjectResult("Network not found.");
                selectednetwork.Configuration = options;

                if (!agent.InitializeRoute(selectednetwork))
                {
                    return new BadRequestObjectResult("One or more network options values are invalid.");
                }
                
                var route = agent.GetNetworkRoute();
                return Ok(route);
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
