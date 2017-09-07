using System;
using System.Collections.Generic;
using System.Text;

namespace NavigationAgent.Resources
{
    public class NetworkSettings
    {
        public Resource NLDI { get; set; }
        public List<Network> Networks{get;set;}
    }
    public class Resource
    {
        public string baseurl { get; set; }
        public Dictionary<string, string> resources { get; set; }    
    }
}
