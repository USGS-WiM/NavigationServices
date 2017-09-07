using System;
using System.Collections.Generic;
using System.Text;
using GeoJSON.Net.Feature;

namespace NavigationAgent.Resources
{
    public class Catchment
    {
        public int comID { get; set; }
        public FeatureCollection Feature { get; set; }
    }
}
