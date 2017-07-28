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
        public void AddDuplicateHeader_ThrowsHeaderAlreadyAddedException()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            // Act + Assert
            Assert.Throws<HeaderAlreadyAddedException>(() =>
            {
                request.AddHeader("HeaderKey", "HeaderValue");
                request.AddHeader("HeaderKey", "OtherValue");
            });
        }
    }
}
