<h1 align="center">fluid http</h1>

<p align="center">
  <a href="https://travis-ci.org/callumevans/fluid-http">
  <img src="https://img.shields.io/travis/callumevans/fluid-http.svg?branch=master&style=flat-square">
  </a>
  
  <a href="https://www.codacy.com/app/callumevans/fluid-http/dashboard">
  <img src="https://img.shields.io/codacy/grade/568f11adbb7340ad83865ca86a636185.svg?style=flat-square">
  </a>
 
  <a href="https://github.com/callumevans/fluid-http/issues">
  <img src="https://img.shields.io/github/issues/callumevans/fluid-http.svg?style=flat-square">
  </a>
  
  <a href="https://github.com/callumevans/fluid-http/blob/master/LICENSE">
  <img src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square">
  </a>
  
  <a href="https://gitter.im/fluid-http/">
  <img src="https://img.shields.io/gitter/room/fluid-http/shields.svg?style=flat-square">
  </a>
</p>

## Familiar

FluidHttp is a .NET HTTP client library heavily inspired by [RestSharp](https://github.com/restsharp/RestSharp) and offers a similar API that's _super_ easy to jump into.

```csharp
var client = new FluidClient("https://localhost.com/");
var request = new FluidRequest("resources/1");

// Append headers to the request!
request.WithHeader("Content-Type", "application/json");

// And a Json or Xml body!
request.WithJsonBody(new { Payload = "Hello world!" });

// And query string parameters!
request.WithQueryParameter("QueryParameter", "query parameter value!");

// Execute the request and parse the response!
MyClass response = await client.FetchAsync<MyClass>();
```

## Fluent

FluidHttp also comes with a few handy extension methods to help you quickly spin up a request without messing about with clients.

```csharp
MyClass myObject = await new FluidRequest("https://localhost.com/resources/1")
	.WithHeader("Content-Type", "application/json")
	.WithBodyParameter("Data", "body content")
	.WithQueryParameter("QueryParameter", "query parameter value!")
	.FetchAsync<MyClass>();
```

## Fast

FluidHttp's `FluidClient` wraps .NET's `HttpClient`, and can happily be used by as many threads as you can throw at it! There isn't really a wrong way to use FluidHttp, but if you'd like to minimise any performance overhead the best way is to use a single `FluidClient` object across your application.

## Flexible

FluidHttp is built with extensibility in mind. You can easily build on existing functionality or add your own with extension methods and good-old OOP!

FluidHttp _should_ work well with any IoC containers, though thoroughly testing this is on the to-do list.
