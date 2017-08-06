﻿using FluidHttp.Client;
using FluidHttp.Exceptions;
using FluidHttp.Request;
using FluidHttp.Response;
using FluidHttp.Tests.Mocks;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class FetchTests
    {
        private readonly Mock<FakeHttpMessageHandler> messageHandler
            = new Mock<FakeHttpMessageHandler>() { CallBase = true };

        private readonly FluidClient client;

        const string contentResponse = "response content!";
        const string url = "http://localhost.com";

        HttpResponseMessage message = new HttpResponseMessage
        {
            Content = new StringContent(contentResponse)
        };

        public FetchTests()
        {
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(message));

            var testClient = new HttpClient(messageHandler.Object);

            client = new FluidClient(testClient);
        }

        (string methodString, HttpMethod methodModel)[] methodsArray = new(string, HttpMethod)[]
        {
                ("GET", HttpMethod.Get),
                ("DELETE", HttpMethod.Delete),
                ("HEAD", HttpMethod.Head),
                ("OPTIONS", HttpMethod.Options),
                ("POST", HttpMethod.Post),
                ("PUT", HttpMethod.Put),
                ("TRACE", HttpMethod.Trace)
        };

        [Fact]
        public async Task Fetch_ReturnsResponse()
        {
            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.IsType<FluidResponse>(response);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Fetch_GetsContentFromUrl()
        {
            // Act
            await client.FetchAsync(url);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.Method == HttpMethod.Get && i.RequestUri == new Uri(url)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_PlacesContentFromResponseInResult()
        {
            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.Equal(contentResponse, response.Content);
        }

        [Theory]
        [InlineData("text/html")]
        [InlineData("application/json")]
        [InlineData("multipart/form-data")]
        public async Task Fetch_PlacesHeadersFromResponseInResult(string responseType)
        {
            // Arrange
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(responseType);

            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.Equal(responseType, response.Headers["Content-Type"]);
        }
        
        [Fact]
        public async Task Fetch_MultipleValuesForHeader_ReturnsCsvInFluidResponse()
        {
            // Arrange
            message.Content.Headers.Add("Test-Header", new string[] { "value1", "value2" });

            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.Equal("value1,value2", response.Headers["Test-Header"]);
        }

        [Fact]
        public async Task Fetch_WithMethod_CallsUrlWithSpecifiedMethod()
        {
            // Act + Assert
            foreach (var method in methodsArray)
            {
                await client.FetchAsync(url, method.methodModel);

                messageHandler
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(i => i.Method == method.methodModel && i.RequestUri == new Uri(url)),
                        ItExpr.IsAny<CancellationToken>());
            }
        }

        [Fact]
        public async Task Fetch_WithStringMethod_ParsesStringAndMakesRequest()
        {
            // Act + Assert
            foreach (var method in methodsArray)
            {
                await client.FetchAsync(url, method.methodString);

                messageHandler
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(i => i.Method == method.methodModel && i.RequestUri == new Uri(url)),
                        ItExpr.IsAny<CancellationToken>());
            }
        }

        [Fact]
        public async Task Fetch_UnknownMethod_SendsAnyway()
        {
            // Act
            await client.FetchAsync(url, "made-up-method");

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.Method == new HttpMethod("made-up-method")),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_ParsesAndExecutesRequest()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.Method == HttpMethod.Get && i.RequestUri == new Uri(url)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_ClientHasBaseUrl_PrependsBaseUrlToRequest()
        {
            // Arrange
            string baseUrl = "http://www.baseurl.com/";
            string resource = "test/resource/1";

            client.BaseUrl = baseUrl;

            // Act
            await client.FetchAsync(resource);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.Method == HttpMethod.Get && i.RequestUri == new Uri(baseUrl + resource)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("not-a-uri")]
        [InlineData("www.test.com")]
        [InlineData("with spaces")]
        [InlineData("http:// www . test .net")]
        public void SetInvalidBaseUrl_ThrowException(string badUri)
        {
            // Act + Assert
            Assert.Throws<BadBaseUriException>(() => client.BaseUrl = badUri);
        }

        [Theory]
        [InlineData("not-a-uri")]
        [InlineData("www.test.com")]
        [InlineData("with spaces")]
        [InlineData("http:// www . test .net")]
        public async Task Fetch_InvalidResourceUrl_NoBaseUrlSet_ThrowException(string badResource)
        {
            // Act + Assert
            await Assert.ThrowsAsync<BadAbsoluteUriException>(
                async () => await client.FetchAsync(badResource));
        }

        [Theory]
        [InlineData("http://www.test.com/?test=123")]
        [InlineData("#")]
        public async Task Fetch_InvalidRelativeResourceUrl_BaseUrlSet_ThrowException(string badResource)
        {
            // Arrange
            client.BaseUrl = "http://test.com/";

            // Act + Assert
            await Assert.ThrowsAsync<BadRelativeUriException>(
                async () => await client.FetchAsync(badResource));
        }

        [Theory]
        [InlineData("?breakIt=false")]
        [InlineData("not-a-uri")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/")]
        [InlineData("/ ")]
        [InlineData(" / ")]
        public async Task Fetch_ValidResourceUrl_ContinuesAsNormal(string goodResource)
        {
            // Arrange
            client.BaseUrl = "http://test.com/";

            // Act
            await client.FetchAsync(goodResource);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Fetch_EmptyBaseUrl_IgnoresBaseUrl(string baseUrl)
        {
            // Arrange
            client.BaseUrl = baseUrl;

            // Act
            await client.FetchAsync(url);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(url)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Fetch_EmptyBaseUrl_IgnoresBaseUrl_InvalidResource_ThrowsException(string baseUrl)
        {
            // Arrange
            client.BaseUrl = baseUrl;

            // Act
            await client.FetchAsync(url);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(url)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("http://localhost.com/ ", "/my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData(" http://localhost.com/", "/my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData(" http://localhost.com/ ", "/my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", " /my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", "/my-test-resource ", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", " /my-test-resource ", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com/", "/my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", "/my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com/", "my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", "my-test-resource", "http://localhost.com/my-test-resource")]
        [InlineData("http://localhost.com", "?query=string", "http://localhost.com?query=string")]
        public async Task Fetch_CorrectlyConcatenateBaseUrlAndResourceUrl(
            string baseUrl, string resourceUrl, string expected)
        {
            // Arrange
            client.BaseUrl = baseUrl;

            // Act
            await client.FetchAsync(resourceUrl);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.Method == HttpMethod.Get && i.RequestUri == new Uri(expected)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("NumberTest", 123)]
        [InlineData("StringTest", "hello+world")]
        public async Task Fetch_RequestHasQueryParameter_BuildQueryString(string key, object value)
        {
            // Arrange
            string expectedUrl = url + $"?{key}={value}";

            FluidRequest request = new FluidRequest();

            request.AddQueryParameter(key, value);

            request.Url = url;

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_RequestHasArrayQueryParameter_BuildQueryString()
        {
            // Arrange
            string expectedUrl = url + "?ArrayTest[]=val1&ArrayTest[]=val2&ArrayTest[]=val3";

            IEnumerable<string> arrayValues = new List<string>
            {
                "val1",
                "val2",
                "val3"
            };

            FluidRequest request = new FluidRequest();

            request.AddQueryParameter("ArrayTest", arrayValues);

            request.Url = url;

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_RequestHasMultipleQueryParameters_BuildQueryString()
        {
            // Arrange
            string baseUrl = "http://localhost.com/";
            string expectedUrl = baseUrl +
                "?TestParameter=True&" +
                "OtherTest=hello%20world!&" +
                "ArrayTest[]=val1&ArrayTest[]=val2&ArrayTest[]=val3&" +
                "NumberTest=123";

            IEnumerable<string> arrayValues = new List<string>
            {
                "val1",
                "val2",
                "val3"
            };

            FluidRequest request = new FluidRequest();

            request.AddQueryParameter("TestParameter", true);
            request.AddQueryParameter("OtherTest", "hello world!");
            request.AddQueryParameter("ArrayTest", arrayValues);
            request.AddQueryParameter("NumberTest", 123);

            request.Url = url;

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_RequestUrlHasQueryParameters_ResourceUrlHasQueryString()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&MyOtherParameter=hello%20mars";

            FluidRequest request = new FluidRequest();

            request.Url = requestUrl;

            request.AddQueryParameter("MyOtherParameter", "hello mars");

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_RequestHasMultipleParametersWithSameName()
        {
            // Arrange
            client.BaseUrl = url;

            string expectedUrl = "http://localhost.com/?Parameter=red&Parameter=blue";

            FluidRequest request = new FluidRequest();

            request.AddQueryParameter("Parameter", "red");
            request.AddQueryParameter("Parameter", "blue");

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("?", "?")]
        [InlineData("?TestVal=", "?TestVal=")]
        [InlineData("?TestVal", "?TestVal=")]
        [InlineData("?TestVal&OtherTest=true", "?TestVal=&OtherTest=true")]
        [InlineData("?TestVal&OtherVal", "?TestVal=&OtherVal=")]
        [InlineData("?=test&OtherVal", "?=test&OtherVal=")]
        public async Task Fetch_AwkwardQueryString_StillSendRequest(string resource, string expected)
        {
            // Arrange
            client.BaseUrl = url;

            // Act
            await client.FetchAsync(resource);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(url + expected)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_DefaultToBaseUrlIfNoUrlProvided()
        {
            // Arrange
            client.BaseUrl = url;

            // Act
            await client.FetchAsync();

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri == new Uri(url)),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_DefaultToBaseUrlIfNoUrlProvided_NoBaseUrl_ThrowsNoUrlException()
        {
            // Act + Assert
            await Assert.ThrowsAsync<NoUrlProvidedException>(
                async () => await client.FetchAsync());
        }

        [Theory]
        [InlineData("http://local host.com")]
        [InlineData("http://local%20host.com")]
        public async Task Fetch_IllegalCharactersInBaseUrl_InvalidAbsoluteUriException(string baseUrl)
        {
            // Act + Assert
            await Assert.ThrowsAsync<BadAbsoluteUriException>(
                async () => await client.FetchAsync(baseUrl));
        }

        [Theory]
        [InlineData(" ", "%20")]
        [InlineData("\"", "%22")]
        [InlineData("%", "%25")]
        [InlineData("<", "%3C")]
        [InlineData(">", "%3E")]
        [InlineData("\\", "%5C")]
        [InlineData("^", "%5E")]
        [InlineData("`", "%60")]
        [InlineData("{", "%7B")]
        [InlineData("|", "%7C")]
        [InlineData("}", "%7D")]
        public async Task Fetch_SubstituteIllegalCharactersInResourceUrl(
            string character, string encoded)
        {
            // Arrange
            client.BaseUrl = url;

            string resource = $"test{character}resource";

            // Act
            await client.FetchAsync(resource);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => i.RequestUri.OriginalString == $"{url}/test{encoded}resource"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("Key", 123)]
        [InlineData("OtherKey", "string+value")]
        public async Task Fetch_WithBodyParameter_AddToRequestBody(string key, object value)
        {
            // Arrange
            client.BaseUrl = url;

            FluidRequest request = new FluidRequest();

            request.AddBodyParameter(key, value);

            string expected = $"{key}={value}";

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.ReadAsStringAsync().Result == expected),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(" ", "%20")]
        [InlineData("\"", "%22")]
        [InlineData("%", "%25")]
        [InlineData("<", "%3C")]
        [InlineData(">", "%3E")]
        [InlineData("\\", "%5C")]
        [InlineData("^", "%5E")]
        [InlineData("`", "%60")]
        [InlineData("{", "%7B")]
        [InlineData("|", "%7C")]
        [InlineData("}", "%7D")]
        public async Task Fetch_WithBodyParameters_EncodeCharacters(
            string character, string encoded)
        {
            // Arrange
            client.BaseUrl = url;

            FluidRequest request = new FluidRequest();

            request.AddBodyParameter("TestValue", character);

            string expected = $"TestValue={encoded}";

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.ReadAsStringAsync().Result == expected),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_WithHeaders()
        {
            // Arrange
            client.BaseUrl = url;

            FluidRequest request = new FluidRequest();

            request.SetHeader("Key", "Value");

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Headers.GetValues("Key").Single() == "Value"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_NoContentTypeHeader_DefaultToWwwFormEncoded()
        {
            // Arrange
            client.BaseUrl = url;

            // Act
            await client.FetchAsync();

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.Headers.ContentType.MediaType == "application/x-www-form-encoded"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("application/x-www-form-encoded")]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        [InlineData("custom/type")]
        public async Task Fetch_ContentTypeSet(string type)
        {
            // Arrange
            client.BaseUrl = url;

            FluidRequest request = new FluidRequest();

            request.ContentType = type;

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.Headers.ContentType.MediaType == type),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}