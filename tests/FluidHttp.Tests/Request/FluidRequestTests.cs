using FluidHttp.Exceptions;
using FluidHttp.Request;
using Xunit;

namespace FluidHttp.Tests.Request
{
    public class FluidRequestTests
    {
        private readonly FluidRequest request;

        public FluidRequestTests()
        {
            request = new FluidRequest();
        }

        [Fact]
        public void SetHeader_OverwritesExistingHeader()
        {
            // Arrange
            request.SetHeader("HeaderKey", "HeaderValue");

            // Act
            request.SetHeader("HeaderKey", "NewHeader!");

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "NewHeader!");
        }

        [Fact]
        public void SetHeader_AddIfHeaderDoesntExist()
        {
            // Act
            request.SetHeader("HeaderKey", "HeaderValue");

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "HeaderValue");
        }
    }
}
