using System;
using System.Collections.Generic;
using System.Text;

namespace NavigationAgent.Resources
{
    public class Input
    {
        public string id { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }
    public class NLDIInput
    {
        public List<Input> inputs { get; set; }

        #region Constructor
        public NLDIInput(string latitude, string longitude)
        {
            this.inputs = new List<Input>
            {
                new Input{ id = "lat", type = "text/plain", value = latitude},
                new Input{ id = "lng", type = "text/plain", value = longitude},
                new Input{ id = "raindroptrace", type = "text/plain", value = "True"},
                new Input{ id = "direction", type = "text/plain", value = "down"}
            };
        }
        #endregion
    }
}
