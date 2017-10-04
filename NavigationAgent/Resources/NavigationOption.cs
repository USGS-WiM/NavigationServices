using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.CoordinateReferenceSystem;

namespace NavigationAgent.Resources
{
    public class NavigationOption
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public string ValueType { get; set; }
        public dynamic Value { get; set; }
        public Boolean ShouldSerializeValue()
        { return Value != null; }

        public static NavigationOption StartPointLocationOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_startpoint,
                Name = "Start point location",
                Description = "Specified lat/long/crs  navigation start location",
                ValueType = "geojson point geometry",

                Value = new Point(new Position(39.79728106, -84.088548)) { CRS = new NamedCRS("EPSG:4326") }
            };
        }
        public static NavigationOption EndPointLocationOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_endpoint,
                Name = "End point location",
                Description = "Specified lat/long/crs  navigation end location",
                ValueType = "point",
                Value = new Point(new Position(-39.79728106, -84.088548)) { CRS = new NamedCRS("EPSG:4326") }
            };
        }
        public static NavigationOption DistanceOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_distance,
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
                ID = (int)navigationoptiontype.e_trunkatingpolygon,
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
                ID = 0,
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
                ID = (int)navigationoptiontype.e_navigationdirection,
                Name = "Direction",
                Description = "Network operation direction",
                ValueType = "option",
                Value = Enum.GetNames(typeof(directiontype)).ToList()
            };
        }
        public static NavigationOption QuerySourceOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_querysource,
                Name = "Query Source",
                Description = "Specified data source to query",
                ValueType = "option",
                Value = Enum.GetNames(typeof(querysourcetype)).ToList()
            };
        }

        public enum querysourcetype
        {
            flowline = 0,
            wqpsite = 1,
            gage = 2,
            bridge = 3
        }
        public enum directiontype
        {
            upstream = 0,
            downstream = 1
        }
        public enum navigationoptiontype
        {
            e_startpoint = 1,
            e_endpoint = 2,
            e_distance = 3,
            e_trunkatingpolygon = 4,
            e_navigationdirection = 5,
            e_querysource = 6
        }
    }
}
