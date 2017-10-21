[![Gitter](https://badges.gitter.im/PhillipPruett/Peaky.svg)](https://gitter.im/PhillipPruett/Peaky?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge) [![Build status](https://ci.appveyor.com/api/projects/status/5ui79hatbw9k5yas/branch/master?svg=true)](https://ci.appveyor.com/project/PhillipPruett/peaky/branch/master)  [![NuGet Status](http://img.shields.io/nuget/v/Peaky.svg?style=flat)](https://www.nuget.org/packages/Peaky/) 

[Try it out here!](http://peaky-sample.azurewebsites.net/tests)

### Peaky Exposes Tests as HTTP Endpoints

Peaky discovers tests in an application and maps routes to allow them to be called over HTTP. These tests can live within the application they are testing or hosted in a standalone service.

### What does Peaky provide me?

One of the great benefits of Peaky is in moving tests away from the antiquated paradigm where tests are run on local machines to being run from a shareable and accessible location. This does away with _‘works on my machine’_ test issues and allows you to easily share, discover, and execute tests.

Peaky also provides users with a CDN-hosted UI out of the box with hooks to provide your own UI experience if you desire.

Peaky aims to remove the barriers between production and pre-production environment tests. With Peaky you register your target environments and their dependencies and let Peaky inject the correct dependency for each test.  E.g. a tests that verifies a homepages availability needs to only be written once and depending on the request to Peaky, it can be ran against any target. 

### What is considered a Peaky test?

Any public method on concrete type derived from `IPeakyTest`. 

There are a few exceptions. Peaky will not expose the following as tests:

* Properties
* Constructors
* Methods with parameters having no default values
* Methods with a [Special Name](https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isspecialname(v=vs.110).aspx)

### Sensors

Peaky will also discover [Its.Log Sensors](https://github.com/jonsequitur/Its.Log) in all loaded assemblies and expose them via HTTP endpoints.

### Examples

Check out the [Peaky WebApplication Sample](https://github.com/PhillipPruett/Peaky/tree/master/Sample/Peaky.SampleWebApplication) to see an example of actual tests being defined and discovered.

#### Marking a test class as a Peaky test class

Any class that implements `IPeakyTest` (or its derived interfaces `IApplyToApplication`, `IApplyToEnvironment`, `IApplyToTarget`, and `IHaveTags`) will have its qualifying methods discovered and exposed as tests.

#### How Peaky Tests Are Written

Write a Peaky test much in the same way that you would write any unit test. A test method that throws an exception will return a 500 Internal Server Error response to the caller, signaling failure, and tests that do not throw an exceptions will return 200 OK, signaling success. 

```csharp
public string bing_homepage_returned_in_under_5ms()
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    httpClient.GetAsync("/").Wait();
    stopwatch.Stop();

    stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);
    return $"{stopwatch.ElapsedMilliseconds} milliseconds";
}
```

#### Routing Tests at Application Startup

Tests will by default be located at `http://yourPeakyApplicationsBaseUri/tests`

```csharp
config.MapTestRoutes(targets => targets.Add("prod","bing", new Uri("https://bing.com")));
```

#### Structure of a Peaky Uri

A peaky URI ( `http://yourPeakyApplicationsBaseUri/tests/{environment}/{application}/{testname}` ) has 3 major parts to it:

* Application: What is the name of the service under test?
* Environment: What environment are we testing that application in, e.g local, production, or internal, or deployment A or deployment B?
* Test Name: the name of the method discovered as a test

These elements combine to form unique test URIs. 

#### Test Discovery

All tests, their tags, and their parameters are discoverable with a query. The following are examples of queries to discover tests:

`HTTP GET /tests` will return all tests in the application
`HTTP GET /tests/{environment}` will return all tests within the requested environment
`HTTP GET /tests/{environment}/{application}` will return all tests for the application within that environment


#### Test Tags

A test class can implement IHaveTags which will allow for categorization of tests and allow users to filter based upon them:

`HTTP GET /tests/{environment}/{application}/?{tag}=true` will only return tests within the test classes with that tag
`HTTP GET /tests/{environment}/{application}/?{tag}=false` will return tests except those within the test classes with that tag

Numerous tags can be filtered on with one request.


#### Dependency Injection

By default, Peaky will allow your test classes to take a dependency on an `HttpClient` only. This is constructed using the details provided at app startup:

```csharp
config.MapTestRoutes(targets => targets.Add("prod","bing", new Uri("https://bing.com")));
```
This would allow any test classes that apply to both 'prod' and 'bing' to depend on an `HttpClient`.

If you have other dependencies, they can be registered as follows:

```csharp
config.MapTestRoutes(targets =>
                     targets.Add("prod",
                                 "bing",
                                 new Uri("https://bing.com"),
                                 registry => registry.Register(new TableStorageClient())
                                                     .Register(new AuthenticatedHttpClient())));
```
