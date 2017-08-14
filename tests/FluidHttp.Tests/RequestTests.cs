using FluidHttp.Request;
using Xunit;

namespace FluidHttp.Tests
{
    public class RequestTests
    {
        [Fact]
        public void InitialiseUrlInConstructor()
        {
            // Arrange
            string url = "http://www.localhost.com/";

            // Act
            FluidRequest request = new FluidRequest(url);

            // Assert
            Assert.Equal(url, request.Url);
        }

        [Fact]
        public void SetHeader_OverwritesExistingHeader()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

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
            FluidRequest request = new FluidRequest();

            request.SetHeader("HeaderKey", "HeaderValue");

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "HeaderValue");
        }
    }
}
