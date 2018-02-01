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

        private const string url = "http://localhost.com";

        public DefaultHeaderTests()
        {
            client = new FluidClient(url, messageHandler);
        }

        [Fact]
        public async Task ClientWithDefaultHeaders_AppendsToRequest()
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            client.SetDefaultHeader("TestHeader", "TestValue");

            // Act
            await client.FetchAsync(request);

            // Assert
            Assert.Equal("TestValue", messageHandler.RequestMessage.Headers.GetValues("TestHeader").Single());
        }

        /// <summary>
        /// Prevents 'Misued Header Name' error from occuring with HttpClient
        /// When adding mismatched headers (ie. Content-Type) to the wrong part of the
        /// HttpClient pipeline it will break. This test verifies it won't be a problem.
        /// </summary>
        [Theory]
        [InlineData("Allow", "GET")]
        [InlineData("Allow", "GET, POST, HEAD")]
        [InlineData("Content-Disposition", "attachment")]
        [InlineData("Content-Disposition", "attachment; filename=\"filename.jpg\"")]
        [InlineData("Content-Encoding", "gzip")]
        [InlineData("Content-Encoding", "gzip, identity")]
        [InlineData("Content-Language", "de-DE")]
        [InlineData("Content-Language", "de-DE, en-CA")]
        [InlineData("Content-Length", "3495")]
        [InlineData("Content-Location", "/documents/foo.json")]
        [InlineData("Content-MD5", "82dbdd5ec5d4aa139b33488e39a670e3")]
        [InlineData("Content-Range", "bytes 200-1000/67589")]
        [InlineData("Content-Type", "text/plain", "text/plain; charset=utf-8")]
        [InlineData("Content-Type", "application/xml", "application/xml; charset=utf-8")]
        [InlineData("Content-Type", "application/json", "application/json; charset=utf-8")]
        [InlineData("Expires", "Wed, 21 Oct 2015 07:28:00 GMT")]
        [InlineData("Last-Modified", "Wed, 21 Oct 2015 07:28:00 GMT")]
        [InlineData("content-encoding", "gzip, identity")]
        [InlineData("content-disposition", "attachment; filename=\"filename.jpg\"")]
        [InlineData("allow", "GET, POST, HEAD")]
        [InlineData("content-type", "text/plain", "text/plain; charset=utf-8")]
        public async Task ClientWithDefaultHeaders_MisusedHeader_WorksAnyway(string reservedHeader, string inputValue, string expectedValue = null)
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            client.SetDefaultHeader(reservedHeader, inputValue);

            // Act
            await client.FetchAsync(request);

            // Assert            
            var allHeaders = messageHandler.RequestHeaders.Concat(messageHandler.ContentHeaders).ToList();
            var header = allHeaders.Single(x => x.Key.ToLower() == reservedHeader.ToLower());
            var headerValue = string.Join(", ", header.Value);
            
            Assert.Equal(expectedValue ?? inputValue, headerValue);
        }
    }
}