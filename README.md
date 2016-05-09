[![Gitter](https://badges.gitter.im/PhillipPruett/Peaky.svg)](https://gitter.im/PhillipPruett/Peaky?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

[![Build status](https://ci.appveyor.com/api/projects/status/5ui79hatbw9k5yas/branch/master?svg=true)](https://ci.appveyor.com/project/PhillipPruett/peaky/branch/master)

### Peaky Exposes Tests as HTTP Endpoints
Peaky discovers tests in an application and maps routes to allow them to be called over HTTP. These tests can live within the application they are testing or stood up as their own service.
### What does Peaky provide me?
One of the great benefits of Peaky is moving tests away from the antiquated paradigm where tests are ran on local machines to being ran from a sharable and accessible location. This does away with _‘works on my machine’_ test issues and allows you to easily share, discover, and execute tests.

Peaky also provides users with a CDN hosted UI out of the box with hooks to provide your own UI experience if you desire.

Peaky aims to remove the barriers between production and pre-production environment tests. With Peaky you register your target environments and their dependencies and let Peaky inject the correct dependency for each test.  E.g. a tests that verifies a homepages availability needs to only be written once and depending on the request to Peaky, it can be ran against any target. 
### What is considered a Peaky test?
Basically any public method on concrete types derived from IPeakyTest. There are a few exceptions to this. Peaky will not expose the following as tests:
* Public properties
* Constructors
* Methods with **non-defaulted** parameters
* Methods with a [Special Name](https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isspecialname(v=vs.110).aspx)

### Sensors
Peaky will also discover [Its.Log Sensors](https://github.com/jonsequitur/Its.Log) in all loaded assemblies and expose them via HTTP endpoints.