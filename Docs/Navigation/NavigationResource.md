### Navigation Resource
Returns the selected Navigation resource and any configuration options required to execute the route method. 

There are 6 different configuration objects required for various resources. Each of which follow a structure similar to the example below:

#### Request Example
The REST URL section below displays the example url and can be queried by selecting the blue "Load response" Button after the required parameters are populated. Table 1 lists a the set of example parameters that can be used to simulate a request.
Table 1. Example of Site Resource services parameter names and values.

| Parameter     | Description   | Value |
| ------------- |:-------------:| -----:|
| CodeOrID    | unique resource code or ID from [Available Navigation Resource](./#/Navigation/GET/AvailableNavigationResources) | 3  |

Resulting in following network trace response:
```
{
   "id":3,
   "code":"networktrace",
   "name":"Network Trace",
   "description":"Upstream or downstream network trace to identify available data selections, such as gages, dams, and/or water quality sites, etc.",
   "configuration":[
      {
         "id":1,
         "name":"Start point location",
         "required":true,
         "description":"Specified lat/long/crs  navigation start location",
         "valueType":"geojson point geometry",
         "value":{
            "type":"Point",
            "coordinates":[
               -84.088548,
               39.79728106
            ],
            "crs":{
               "properties":{
                  "name":"EPSG:4326"
               },
               "type":"name"
            }
         }
      },
      {
         "id":5,
         "name":"Direction",
         "required":true,
         "description":"Network operation direction",
         "valueType":"exclusiveOption",
         "value":[
            "downstream",
            "upstream"
         ]
      },
      {
         "id":6,
         "name":"Query Source",
         "required":true,
         "description":"Specified data source to query",
         "valueType":"option",
         "value":[
            "flowline",
            "wqpsite",
            "gage",
            "bridge"
         ]
      },
      {
         "id":0,
         "name":"Limit",
         "required":false,
         "description":"Limits network operations to within specified option",
         "valueType":"exclusiveOption",
         "value":[
            {
               "id":3,
               "name":"Distance (km)",
               "description":"Limiting distance in kilometers from starting point",
               "valueType":"numeric",
               "value":10
            },
            {
               "id":4,
               "name":"Polygon geometry",
               "description":"Limits network operations to within specified feature",
               "valueType":"geojson polygon or multipolygon geometry used to limit the response. See https://tools.ietf.org/html/rfc7946#section-3.1.6 for more details",
               "value":{
                  "type":"Polygon",
                  "coordinates":[
                     [
                        [
                           -72.956256866455078,
                           42.430932367025328
                        ],
                        [
                           -72.8532600402832,
                           42.430932367025328
                        ],
                        [
                           -72.8532600402832,
                           42.501338949730247
                        ],
                        [
                           -72.956256866455078,
                           42.501338949730247
                        ],
                        [
                           -72.956256866455078,
                           42.430932367025328
                        ]
                     ]
                  ]
               }
            }
         ]
      }
   ],
   "links":[
      {
         "rel":"Route the networks using supplied configuration options.",
         "href":"test.streamstats.usgs.gov/navigation/3/route",
         "method":"POST"
      }
   ]
}
```