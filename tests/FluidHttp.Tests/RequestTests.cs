using System.Linq;
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

            request.Headers["HeaderKey"] = "HeaderValue";

            // Act
            request.Headers["HeaderKey"] = "NewHeader!";

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "NewHeader!");
        }

        [Fact]
        public void SetHeader_AddIfHeaderDoesntExist()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            // Act
            request.Headers.Add("HeaderKey", "HeaderValue");

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "HeaderValue");
        }

        [Fact]
        public void AddParameter_AddsToCollection()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            // Act
            request.AddParameter("MyParameter", "test", ParameterType.Query);

            // Assert
            Assert.Contains(request.Parameters, x => x.Name == "MyParameter");
        }
    }
}
