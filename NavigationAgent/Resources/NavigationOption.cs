using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Converters;
using Newtonsoft.Json;

namespace NavigationAgent.Resources
{
    public class NavigationOption
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public Boolean? Required { get; set; }
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
                Required = true,
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
                Required = true,
                Description = "Specified lat/long/crs  navigation end location",
                ValueType = "geojson point geometry",
                Value = new Point(new Position(39.79728106, -84.088548)) { CRS = new NamedCRS("EPSG:4326") }
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
                Value = 10
            };
        }
        public static NavigationOption PolygonOption()
        {
            var geojsonPolygon = @"{""type"": ""Polygon"",""coordinates"": [[
                                        [-72.95625686645508,42.43093236702533],
                                        [-72.8532600402832,42.43093236702533],
                                        [-72.8532600402832,42.50133894973025],
                                        [-72.95625686645508,42.50133894973025],
                                        [-72.95625686645508,42.43093236702533]
                                        ]]}";

            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_trunkatingpolygon,
                Name = "Polygon geometry",
                Description = "Limits network operations to within specified feature",
                ValueType = "geojson polygon or multipolygon geometry used to limit the response. See https://tools.ietf.org/html/rfc7946#section-3.1.6 for more details",
                Value = JsonConvert.DeserializeObject<Polygon>(geojsonPolygon, new GeometryConverter())
            };
        }
        public static NavigationOption LimitOption()
        {
            return new NavigationOption()
            {
                ID = 0,
                Name = "Limit",
                Required = false,
                Description = "Limits network operations to within specified option",
                ValueType = "exclusiveOption",
                Value = new List<NavigationOption>() { DistanceOption(), PolygonOption() }
            };
        }
        public static NavigationOption DirectionOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_navigationdirection,
                Name = "Direction",
                Required = true,
                Description = "Network operation direction",
                ValueType = "exclusiveOption",
                Value = Enum.GetNames(typeof(directiontype)).ToList()
            };
        }
        public static NavigationOption QuerySourceOption()
        {
            return new NavigationOption()
            {
                ID = (int)navigationoptiontype.e_querysource,
                Name = "Query Source",
                Required = true,
                Description = "Specified data source to query",
                ValueType = "option",
                Value = Enum.GetNames(typeof(querysourcetype)).ToList()
            };
        }

        public enum querysourcetype
        {
            //int should match nldi querytype
            flowline = 0,
            wqpsite = 1,
            streamStatsgage = 2,
            nwisgage=3
        }
        public enum directiontype
        {
            //int should match nldi direction type
            downstream = 1,
            upstream = 2,
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
