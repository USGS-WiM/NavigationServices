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

        public Feature catchment { get; set; }

        public FeatureCollection FeatureCollection { get; set; }
      
        public List<NavigationOption> Configuration { get; set; }

        #region Constructor
        public Route(NavigationAgent.navigationtype type, List<NavigationOption> configlist)
        {
            this.Type = type;
            this.Configuration = configlist;
            FeatureCollection = new FeatureCollection();
        }
        public Route():this(NavigationAgent.navigationtype.e_flowpath,null)
        {        }
        #endregion

    }
}
