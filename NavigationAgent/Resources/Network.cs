using System;
using System.Collections.Generic;
using WiM.Hypermedia;


using System.Text;

namespace NavigationAgent.Resources
{
    public class Network:IHypermedia
    {
        public Int32 ID { get; set; }
        public string Code { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<NavigationOption> Configuration { get; set; }
        public List<Link> Links { get; set; }

        public Boolean ShouldSerializeConfiguration()
        { return Configuration != null && Configuration.Count > 0; }
    }
}
