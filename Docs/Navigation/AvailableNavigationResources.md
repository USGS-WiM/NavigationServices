## Available Navigation Resources
Returns an array of available navigation resources currently provided by the services

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
