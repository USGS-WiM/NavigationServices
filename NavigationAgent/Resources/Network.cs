using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;

using System.Text;

namespace NavigationAgent.Resources
{
    public class Network
    {
        public Int32 ID { get; set; }
        public string Code { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<NavigationOption> Configuration { get; set; }
        public Boolean ShouldSerializeConfiguration()
        { return Configuration != null && Configuration.Count > 0; }
    }

    public class NavigationOption
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public string ValueType { get; set; }
        public dynamic Value { get; set; }
        public Boolean ShouldSerializeValue()
        { return Value != null; }

        public static NavigationOption StartPointLocationOption() {
            return new NavigationOption()
            {
                ID = 1,
                Name = "Start point location",
                Description = "Specified lat/long/srid  navigation start location",
                ValueType = "geojson point geometry",
                
                Value = new Point(new Position(0, 0))
            };
        }
        public static NavigationOption EndPointLocationOption()
        {
            return new NavigationOption()
            {
                ID = 2,
                Name = "End point location",
                Description = "Specified lat/long/srid  navigation end location",
                ValueType = "point",
                Value = new Point(new Position(0,0))
            };
        }
        public static NavigationOption DistanceOption()
        {
            return new NavigationOption()
            {
                ID = 3,
                Name = "Distance (km)",
                Description = "Limiting distance in kilometers from starting point",
                ValueType = "numeric",
                Value = "undefined"
            };
        }
        public static NavigationOption PolygonOption()
        {
            return new NavigationOption()
            {
                ID = 4,
                Name = "Polygon",
                Description = "Limits network operations to within specified polygon",
                ValueType = "geojson polygon geometry",
                Value = "undefined"
            };
        }
        public static NavigationOption LimitOption()
        {
            return new NavigationOption()
            {
                ID = 4,
                Name = "Limit",
                Description = "Limits network operations to within specified option",
                ValueType = "option",
                Value = new List<NavigationOption>() { DistanceOption(), PolygonOption() }
            };
        }
        public static NavigationOption DirectionOption()
        {
            return new NavigationOption()
            {
                ID = 4,
                Name = "Direction",
                Description = "Network operation direction",
                ValueType = "option",
                Value = new List<String>() { "upstream", "downstream" }
            };
        }
        public static NavigationOption QuerySourceOption()
        {
            return new NavigationOption()
            {
                ID = 4,
                Name = "Query Source",
                Description = "Specified data source to query",
                ValueType = "option",
                Value = Enum.GetValues(typeof(querysourcetype)).Cast<querysourcetype>().ToList()
        };
        }

        public enum querysourcetype
        {
            flowline = 0,
            wqpsite = 1,
            gage = 2,
            bridge = 3

        }
    }
}
