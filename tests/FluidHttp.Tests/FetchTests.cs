using FluidHttp.Exceptions;
using FluidHttp.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class FetchTests
    {
        private readonly FakeHttpMessageHandler messageHandler;
        private readonly FluidClient clientWithUrl;
        private readonly FluidClient clientNoUrl;

        private const string contentResponse = "response content!";
        private const string baseUrl = "http://localhost.com";

        public FetchTests()
        {
            messageHandler = new FakeHttpMessageHandler();
            clientWithUrl = new FluidClient(baseUrl, messageHandler);
            clientNoUrl = new FluidClient(messageHandler);
        }

        private readonly (string methodString, HttpMethod methodModel)[] methodsArray = {
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
            IFluidResponse response = await clientNoUrl.FetchAsync(baseUrl);

            // Assert
            Assert.IsType<FluidResponse>(response);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Fetch_GetsContentFromUrl()
        {
            // Act
            await clientNoUrl.FetchAsync(baseUrl);

            // Assert
            Assert.Equal(new Uri(baseUrl), messageHandler.RequestUrl);
            Assert.Equal(HttpMethod.Get, messageHandler.RequestMethod);
        }

        [Fact]
        public async Task Fetch_PlacesContentFromResponseInResult()
        {
            // Act
            IFluidResponse response = await clientNoUrl.FetchAsync(baseUrl);

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
            messageHandler.ResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(responseType);

            // Act
            IFluidResponse response = await clientNoUrl.FetchAsync(baseUrl);

            // Assert
            Assert.Equal(responseType, response.Headers["Content-Type"]);
        }
        
        [Fact]
        public async Task Fetch_MultipleValuesForHeader_ReturnsCsvInFluidResponse()
        {
            // Arrange
            messageHandler.ResponseMessage.Content.Headers.Add(
                "Test-Header", new string[] { "value1", "value2" });

            // Act
            IFluidResponse response = await clientNoUrl.FetchAsync(baseUrl);

            // Assert
            Assert.Equal("value1,value2", response.Headers["Test-Header"]);
        }

        [Fact]
        public async Task Fetch_WithMethod_CallsUrlWithSpecifiedMethod()
        {
            // Act + Assert
            foreach (var method in methodsArray)
            {
                await clientNoUrl.FetchAsync(baseUrl, method.methodModel);

                Assert.Equal(method.methodModel, messageHandler.RequestMethod);
                Assert.Equal(new Uri(baseUrl), messageHandler.RequestUrl);
            }
        }

        [Fact]
        public async Task Fetch_WithStringMethod_ParsesStringAndMakesRequest()
        {
            // Act + Assert
            foreach (var method in methodsArray)
            {
                await clientNoUrl.FetchAsync(baseUrl, method.methodString);

                Assert.Equal(method.methodModel, messageHandler.RequestMethod);
                Assert.Equal(new Uri(baseUrl), messageHandler.RequestUrl);
            }
        }

        [Fact]
        public async Task Fetch_UnknownMethod_SendsAnyway()
        {
            // Act
            await clientNoUrl.FetchAsync(baseUrl, "made-up-method");

            // Assert
            Assert.Equal(new HttpMethod("made-up-method"), messageHandler.RequestMethod);
        }

        [Fact]
        public async Task Fetch_ParsesAndExecutesRequest()
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            request.Url = baseUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(HttpMethod.Get, messageHandler.RequestMethod);
            Assert.Equal(new Uri(baseUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_ClientHasBaseUrl_PrependsBaseUrlToRequest()
        {
            // Arrange
            string resource = "/test/resource/1";

            // Act
            await clientWithUrl.FetchAsync(resource);

            // Assert
            Assert.Equal(HttpMethod.Get, messageHandler.RequestMethod);
            Assert.Equal(new Uri(baseUrl + resource), messageHandler.RequestUrl);
        }

        [Theory]
        [InlineData("not-a-uri")]
        [InlineData("www.test.com")]
        [InlineData("with spaces")]
        [InlineData("http:// www . test .net")]
        public void SetInvalidBaseUrl_ThrowException(string badUri)
        {
            // Act + Assert
            Assert.Throws<BadBaseUriException>(() => new FluidClient(badUri));
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
                async () => await clientNoUrl.FetchAsync(badResource));
        }

        [Theory]
        [InlineData("http://www.test.com/?test=123")]
        [InlineData("#")]
        public async Task Fetch_InvalidRelativeResourceUrl_BaseUrlSet_ThrowException(string badResource)
        {
            // Act + Assert
            await Assert.ThrowsAsync<BadRelativeUriException>(
                async () => await clientWithUrl.FetchAsync(badResource));
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
            // Act
            await clientWithUrl.FetchAsync(goodResource);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Fetch_EmptyBaseUrl_IgnoresBaseUrl(string baseUrl)
        {
            // Act
            await clientNoUrl.FetchAsync(FetchTests.baseUrl);

            // Assert
            Assert.Equal(new Uri(FetchTests.baseUrl), messageHandler.RequestUrl);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Fetch_EmptyBaseUrl_IgnoresBaseUrl_InvalidResource_ThrowsException(string baseUrl)
        {
            // Act + Assert
            await Assert.ThrowsAsync<BadAbsoluteUriException>(
                async () => await clientNoUrl.FetchAsync("/test-resource"));
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
            // Act
            await clientWithUrl.FetchAsync(resourceUrl);

            // Assert
            Assert.Equal(HttpMethod.Get, messageHandler.RequestMethod);
            Assert.Equal(new Uri(expected), messageHandler.RequestUrl);
        }

        [Theory]
        [InlineData("NumberTest", 123)]
        [InlineData("StringTest", "hello+world")]
        public async Task Fetch_RequestHasQueryParameter_BuildQueryString(string key, object value)
        {
            // Arrange
            string expectedUrl = baseUrl + $"?{key}={value}";

            FluidRequest request = new FluidRequest();

            request.WithQueryParameter(key, value);

            request.Url = baseUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_RequestHasArrayQueryParameter_BuildQueryString()
        {
            // Arrange
            string expectedUrl = baseUrl + "?ArrayTest[]=val1&ArrayTest[]=val2&ArrayTest[]=val3";

            IEnumerable<string> arrayValues = new List<string>
            {
                "val1",
                "val2",
                "val3"
            };

            FluidRequest request = new FluidRequest();

            request.WithQueryParameter("ArrayTest", arrayValues);

            request.Url = baseUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_RequestHasMultipleQueryParameters_BuildQueryString()
        {
            // Arrange
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

            request.WithQueryParameter("TestParameter", true);
            request.WithQueryParameter("OtherTest", "hello world!");
            request.WithQueryParameter("ArrayTest", arrayValues);
            request.WithQueryParameter("NumberTest", 123);

            request.Url = baseUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_RequestUrlHasQueryParameters_ResourceUrlHasQueryString()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&MyOtherParameter=hello%20mars";

            FluidRequest request = new FluidRequest();
            
            request.Url = requestUrl;
            request.WithQueryParameter("MyOtherParameter", "hello mars");

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_RequestUrlHasQueryParameters_SetUrlAfterParams_ResourceUrlHasCorrectQueryString()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&MyOtherParameter=hello%20mars";

            FluidRequest request = new FluidRequest();
            
            request.WithQueryParameter("MyOtherParameter", "hello mars");
            request.Url = requestUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }
        
        [Fact]
        public async Task Fetch_SetUrlMultipleTimes_CorrectUrl()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&MyOtherParameter=hello%20mars";

            FluidRequest request = new FluidRequest();
            
            request.WithQueryParameter("MyOtherParameter", "hello mars");
            request.Url = requestUrl;
            request.Url = requestUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }        
        
        [Fact]
        public async Task Fetch_SetUrlThenParamsMultipleTimes_CorrectUrl()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&MyOtherParameter=hello%20mars&ThirdParam=hello%20three";

            FluidRequest request = new FluidRequest();
            
            request.WithQueryParameter("MyOtherParameter", "hello mars");
            request.Url = requestUrl;
            request.WithQueryParameter("ThirdParam", "hello three");
            request.Url = requestUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
        }        
        
        [Fact]
        public async Task Fetch_SetUrlThenBodyParameter_CorrectBody()
        {
            // Arrange
            string requestUrl = "http://localhost.com/?MyParameter=hello%20world";
            string expectedUrl = "http://localhost.com/?MyParameter=hello%20world&QueryParameter=query";

            FluidRequest request = new FluidRequest(HttpMethod.Post);
            
            request.WithQueryParameter("QueryParameter", "query");
            request.Url = requestUrl;
            request.WithBodyParameter("BodyParameter", "body");
            request.Url = requestUrl;
            request.Url = requestUrl;

            // Act
            await clientNoUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
            Assert.Equal("BodyParameter=body", messageHandler.SentMessageContent);
        }

        [Fact]
        public async Task Fetch_RequestHasMultipleParametersWithSameName()
        {
            string expectedUrl = "http://localhost.com/?Parameter=red&Parameter=blue";

            FluidRequest request = new FluidRequest();

            request.WithQueryParameter("Parameter", "red");
            request.WithQueryParameter("Parameter", "blue");

            // Act
            await clientWithUrl.FetchAsync(request);

            // Assert
            Assert.Equal(new Uri(expectedUrl), messageHandler.RequestUrl);
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
            // Act
            await clientWithUrl.FetchAsync(resource);

            // Assert
            Assert.Equal(new Uri(baseUrl + expected), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_DefaultToBaseUrlIfNoUrlProvided()
        {
            // Act
            await clientWithUrl.FetchAsync();

            // Assert
            Assert.Equal(new Uri(baseUrl), messageHandler.RequestUrl);
        }

        [Fact]
        public async Task Fetch_DefaultToBaseUrlIfNoUrlProvided_NoBaseUrl_ThrowsNoUrlException()
        {
            // Act + Assert
            await Assert.ThrowsAsync<NoUrlProvidedException>(
                async () => await clientNoUrl.FetchAsync());
        }

        [Theory]
        [InlineData("http://local host.com")]
        [InlineData("http://local%20host.com")]
        public async Task Fetch_IllegalCharactersInFetchUrl_InvalidAbsoluteUriException(string baseUrl)
        {
            // Act + Assert
            await Assert.ThrowsAsync<BadAbsoluteUriException>(
                async () => await clientNoUrl.FetchAsync(baseUrl));
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
            string resource = $"test{character}resource";

            // Act
            await clientWithUrl.FetchAsync(resource);

            // Assert
            Assert.Equal(new Uri($"{baseUrl}/test{encoded}resource"), messageHandler.RequestUrl);
        }

        [Theory]
        [InlineData("Key", 123)]
        [InlineData("OtherKey", "string+value")]
        public async Task Fetch_WithBodyParameter_AddToRequestBody(string key, object value)
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            request.WithBodyParameter(key, value);

            string expected = $"{key}={value}";

            // Act
            await clientWithUrl.FetchAsync(request);

            // Assert
            Assert.Equal(expected, messageHandler.SentMessageContent);
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
            FluidRequest request = new FluidRequest();
            request.WithBodyParameter("TestValue", character);

            string expected = $"TestValue={encoded}";

            // Act
            await clientWithUrl.FetchAsync(request);

            // Assert
            Assert.Equal(expected, messageHandler.SentMessageContent);
        }

        [Fact]
        public async Task Fetch_WithHeaders()
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            request.Headers["Key"] = "Value";

            // Act
            await clientWithUrl.FetchAsync(request);

            // Assert
            Assert.Equal("Value", messageHandler.RequestHeaders.GetValues("Key").Single());
        }

        [Fact]
        public async Task Fetch_NoContentTypeHeader_DefaultToWwwFormEncoded()
        {
            // Act
            await clientWithUrl.FetchAsync();

            // Assert
            Assert.Equal("application/x-www-form-encoded", messageHandler.ContentHeaders.ContentType.MediaType);
        }

        [Theory]
        [InlineData("application/x-www-form-encoded")]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        [InlineData("custom/type")]
        public async Task Fetch_ContentTypeSet(string type)
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            request.ContentType = type;

            // Act
            await clientWithUrl.FetchAsync(request);

            // Assert
            Assert.Equal(type, messageHandler.ContentHeaders.ContentType.MediaType);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task Fetch_ReturnStatusCodeWithResponse(HttpStatusCode statusCode)
        {
            // Arrange
            messageHandler.ResponseMessage.StatusCode = statusCode;

            // Act
            IFluidResponse response = await clientWithUrl.FetchAsync();

            // Assert
            Assert.Equal(statusCode, response.StatusCode);
        }
    }
}
