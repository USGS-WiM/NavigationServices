The Navigation resource provides the network connectivity components required for network navigation.

#### Navigation Usage
The Navigation resources are intended to be used in the following sequential steps:


1) Request the [Available Navigation Resources](./#/Navigation/GET/AvailableNavigationResources)

2) Using the ID or code from step 1, request [Navigation Resource](./#/Navigation/GET/NavigationResource)

3) Configure the configuration object response from step 2 and use as the requesting body for [Navigation Resource Feature Route](./#/Navigation/UNSPECIFIED/NavigationResourceFeatureRoute). See step 2 for more information regarding configuration response.

