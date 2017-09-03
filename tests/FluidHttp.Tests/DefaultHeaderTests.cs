using FluidHttp.Tests.Mocks;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class DefaultHeaderTests
    {
        private readonly FakeHttpMessageHandler messageHandler = new FakeHttpMessageHandler();
        private readonly FluidClient client;

        const string url = "http://localhost.com";

        public DefaultHeaderTests()
        {
            client = new FluidClient(messageHandler);
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
            Assert.Equal("TestValue", messageHandler.RequestMessage.Headers.GetValues("TestHeader").Single());
        }
    }
}
