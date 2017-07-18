using FluidHttp.Client;
using FluidHttp.Request;
using FluidHttp.Response;
using FluidHttp.Tests.Abstractions;
using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests.Client
{
    public class FluidClientTests
    {
        private readonly Mock<FakeHttpMessageHandler> messageHandler = new Mock<FakeHttpMessageHandler>() { CallBase = true };

        private readonly FluidClient client;

        public FluidClientTests()
        {
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(message));

            var testClient = new HttpClient(messageHandler.Object);

            client = new FluidClient(testClient);
        }

        const string contentResponse = "response content!";
        const string url = "http://localhost.com";

        HttpResponseMessage message = new HttpResponseMessage
        {
            Content = new StringContent(contentResponse)
        };

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
        public async Task FetchUrl_ReturnsResponse()
        {
            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.IsType<FluidResponse>(response);
            Assert.NotNull(response);
        }
        
        [Fact]
        public async Task FetchUrl_GetsContentFromUrl()
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
        public async Task FetchUrl_PlacesContentFromResponseInResult()
        {
            // Act
            FluidResponse response = await client.FetchAsync(url);

            // Assert
            Assert.Equal(contentResponse, response.Content);
        }

        [Fact]
        public async Task FetchUrl_WithMethod_CallsUrlWithSpecifiedMethod()
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
        public async Task FetchUrl_WithStringMethod_ParsesStringAndMakesRequest()
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
        public async Task FetchUrl_UnknownMethod_SendsAnyway()
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
        public async Task FetchAsync_ParsesAndExecutesRequest()
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
    }
}
