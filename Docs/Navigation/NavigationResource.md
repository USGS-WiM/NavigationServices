### Navigation Resource
Returns the selected Navigation resource and any configuration options required to execute the route method. 

There are 6 different configuration objects required for various resources. Each of which follow the following structure:
```
{
	"id":1,
	"name":"Start point location",
	"required":true,
	"description":"Specified lat/long/crs  navigation start location",
	"valueType":"geojson point geometry",
	"value":{"type":"Point","coordinates":[-72.87935256958009,42.45284793716157],"crs":{"properties":{"name":"EPSG:4326"},"type":"name"}}
}
```

Required and optional configuration is returned by each response as shown in the following sample or by selecting the below load response button below;
#### Request
``` 
	$.ajax({
		url: url,
		success: function(resultData) { 
			var geojson = resultData;
		},
		error: function() {
			control.state('error');
		}
	});
```
