using System;
using System.Collections.Generic;
using System.Text;
using GeoJSON.Net.Feature;

namespace NavigationAgent.Resources
{
    public class Output
    {
        public string id { get; set; }
        public FeatureCollection value { get; set; }
    }
    public class NLDIOutput
    {
        public List<Output> outputs { get; set; }
    }
}
