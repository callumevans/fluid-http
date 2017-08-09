using FluidHttp.Client;
using FluidHttp.Request;
using FluidHttp.Tests.Mocks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class AddBodyTests
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

        public AddBodyTests()
        {
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(message));

            var testClient = new HttpClient(messageHandler.Object);

            client = new FluidClient(testClient);
        }

        [Fact]
        public async Task Fetch_AddJsonBody_SerializesContentAndSetsType()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;

            object bodyContent = new
            {
                Name = "Test name",
                Age = 123
            };

            request.SetJsonBody(bodyContent);

            // Act
            await client.FetchAsync(request);

            // Assert
            string jsonContent = JsonConvert.SerializeObject(bodyContent);

            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i => 
                        i.Content.ReadAsStringAsync().Result == jsonContent &&
                        i.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_AddJsonBody_IgnoreBodyParametersWhenBodySet()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;

            request.AddBodyParameter("Test", "TestValue");
            
            object bodyContent = new
            {
                Name = "Test name",
                Age = 123
            };

            request.SetJsonBody(bodyContent);

            // Act
            await client.FetchAsync(request);

            // Assert
            string jsonContent = JsonConvert.SerializeObject(bodyContent);

            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.ReadAsStringAsync().Result == jsonContent &&
                        i.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Fetch_AddJsonBodyThenClear_ClearBodyContent()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;

            request.AddBodyParameter("Test", "TestValue");

            object bodyContent = new
            {
                Name = "Test name",
                Age = 123
            };

            request.SetJsonBody(bodyContent);

            request.Body = string.Empty;

            // Act
            await client.FetchAsync(request);

            // Assert
            string jsonContent = JsonConvert.SerializeObject(bodyContent);

            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Content.ReadAsStringAsync().Result == "Test=TestValue"),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
