//------------------------------------------------------------------------------
//----- HttpController ---------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

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
using WiM.Resources;

namespace NavigationServices.Controllers
{
    [Route("[controller]")]
    public class NavigationController : WiM.Services.Controllers.ControllerBase
    {
        public INavigationAgent agent { get; set; }
        public NavigationController(INavigationAgent agent ) : base()
        {
            this.agent = agent;
        }
        #region METHODS
        [HttpGet()]
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
        [HttpGet("{CodeOrID}")]
        public async Task<IActionResult> Get(string CodeOrID)
        {
            //returns list of available Navigations
            try
            {
                
                var selectednetwork = agent.GetNetwork(CodeOrID);
                if (selectednetwork == null) return new BadRequestObjectResult("Network not found.");
                sm(agent.Messages);
                return Ok(selectednetwork);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("{CodeOrID}/Route")]
        public async Task<IActionResult> Route(string CodeOrID, [FromBody]List<NavigationOption> options)
        {
            try
            {
                var selectednetwork = agent.GetNetwork(CodeOrID);
                if (selectednetwork == null) return new BadRequestObjectResult("Network not found.");
                selectednetwork.Configuration = options;   
                

                if(!agent.InitializeRoute(selectednetwork)) return new BadRequestObjectResult("One or more network options values are invalid.");
                var route = agent.GetNetworkRoute();
                sm(agent.Messages);
                return Ok(route);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        #endregion
        #region HELPER METHODS
        private void sm(List<Message> messages)
        {
            HttpContext.Items[WiM.Services.Middleware.X_MessagesExtensions.msgKey] = messages;
        }
        #endregion
    }
}
