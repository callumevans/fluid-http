using FluidHttp.Tests.Mocks;
using Moq;
using Moq.Protected;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class DefaultHeaderTests
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

        public DefaultHeaderTests()
        {
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(message));

            client = new FluidClient(messageHandler.Object);
        }

        [Fact]
        public async Task ClientWithDefaultHeaders_AppendsToRequest()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            client.BaseUrl = url;
            client.SetDefaultHeader("TestHeader", "TestValue");

            // Act
            await client.FetchAsync(request);

            // Assert
            messageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(i =>
                        i.Headers.GetValues("TestHeader").Single() == "TestValue"),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
