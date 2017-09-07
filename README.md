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

FluidHttp is heavily inspired by [RestSharp](https://github.com/restsharp/RestSharp) and offers a similar API that's _super_ easy to jump into.

```csharp
var client = new FluidClient("https://jsonplaceholder.typicode.com");
var request = new FluidRequest("posts/1");

// Append headers to the request!
request.WithHeader("Content-Type", "application/json");

// and body parameters!
request.WithBodyParameter("Data", "body content!");

// and query string parameters!
request.WithQueryParameter("QueryParameter", "query parameter value!");

// Execute the request
FluidResponse response = await client.FetchAsync(request);

// Map the response to an object
Post post = response.ParseResponse<Post>();
```

## Fluent

FluidHttp also comes with a few handy extension methods to help you quickly spin up a request without messing about with clients.

```csharp
Post post = (await
    new FluidRequest("https://jsonplaceholder.typicode.com/posts/1")
    .WithHeader("Content-Type", "application/json")
    .WithBodyParameter("Data", "body content")
    .WithQueryParameter("QueryParameter", "query parameter value!")
    .FetchAsync())
    .ParseResponse<Post>();
```

## Fast

FluidHttp's `FluidClient` wraps .NET's `HttpClient`, and can happily be used by as many threads as you can throw at it! There isn't really a wrong way to use FluidHttp, but if you'd like to minimise any performance overhead the best way is to use a single `FluidClient` object across your application.

## Flexible

FluidHttp is built with extensibility in mind. You can easily build on existing functionality or add your own with extension methods and good-old OOP!

FluidHttp _should_ work well with any IoC containers, though thoroughly testing this is on the to-do list.
