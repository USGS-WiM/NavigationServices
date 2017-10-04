using System;
using System.Collections.Generic;
using System.Text;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace NavigationAgent.Resources
{
    public class Route
    {
        public NavigationAgent.navigationtype Type { get; set; }

        public string ComID { get; set; }

        public Dictionary<string,Feature> Features { get; set; }
      
        public List<NavigationOption> Configuration { get; internal set; }

        #region Constructor
        public Route(NavigationAgent.navigationtype type)
        {
            this.Type = type;
            Features = new Dictionary<string, Feature>();
        }
        public Route():this(NavigationAgent.navigationtype.e_flowpath)
        {        }
        #endregion

    }
   
}
