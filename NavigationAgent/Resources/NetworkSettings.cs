using System;
using System.Collections.Generic;
using System.Text;
using WIM.Utilities.Resources;

namespace NavigationAgent.Resources
{
    public class NetworkSettings
    {
        public string DBConnectionString { get; set; }
        public Resource NLDI { get; set; }
        public List<Network> Networks {get;set;}
        public Resource StreamStats { get; set; }
    }
}
