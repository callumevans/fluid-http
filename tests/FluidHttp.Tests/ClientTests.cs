using FluidHttp.Client;
using Xunit;

namespace FluidHttp.Tests
{
    public class ClientTests
    {
        [Fact]
        public void InitialiseUrlInConstructor()
        {
            // Arrange
            string url = "http://www.localhost.com/";

            // Act
            FluidClient client = new FluidClient(url);

            // Assert
            Assert.Equal(url, client.BaseUrl);
        }
    }
}
