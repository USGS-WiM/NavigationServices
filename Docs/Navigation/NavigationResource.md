### Navigation Resource
Returns the selected Navigation resource and any configuration options required to execute the route method. 

There are currently six different configuration objects required for various resources.
The configuration body is used, instead of url parameters, in order to submit large lists of configuration options for the request.  

#### Request Example
The REST URL section below displays the example url and can be queried by selecting the blue "Load response" Button after the required parameters are populated. Table 1 lists a the set of example parameters that can be used to simulate a request.
Table 1. Example of Site Resource services parameter names and values.

| Parameter     | Description   | Value |
| ------------- |:-------------:| -----:|
| CodeOrID    | unique resource code or ID from [Available Navigation Resource](./#/Navigation/GET/AvailableNavigationResources) | 3  |

Resulting in a response result that includes a list of configuration objects that (if required, is intended to be populated and returned to the [Navigation Resource Feature Route](./#/Navigation/UNSPECIFIED/NavigationResourceFeatureRoute) URL.
