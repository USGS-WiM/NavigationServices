//------------------------------------------------------------------------------
//----- NavigationHypermedia ---------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   Intersects the pipeline after
//
//discussion:   Controllers are objects which handle all interaction with resources. 
//              
//
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiM.Hypermedia;
using WiM.Services.Filters;
using NavigationAgent.Resources;

namespace NavigationServices.Filters
{
    public class NavigationHypermedia : HypermediaBase
    {
        protected override List<Link> GetEnumeratedHypermedia(IHypermedia entity)
        {
            List<Link> results = null;
            switch (entity.GetType().Name)
            {
                case "Network":
                    results = new List<Link>();
                    results.Add(new Link(BaseURI, "self by id", this.URLQuery +"/"+ ((Network)entity).ID, WiM.Resources.refType.GET));
                    results.Add(new Link(BaseURI, "self by code", this.URLQuery + "/" + ((Network)entity).Code, WiM.Resources.refType.GET));
                    break;

                default:
                    break;
            }

            return results;

        }

        protected override List<Link> GetReflectedHypermedia(IHypermedia entity)
        {
            List<Link> results = null;
            switch (entity.GetType().Name)
            {
                case "Network":
                    results = new List<Link>();
                    results.Add(new Link(BaseURI, "Route the networks using supplied configuration options.", this.URLQuery + "/route", WiM.Resources.refType.POST));

                    break;                
                default:
                    break;
            }

            return results;
        }
    }
}
