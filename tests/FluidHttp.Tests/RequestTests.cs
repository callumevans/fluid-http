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

            request.SetHeader("HeaderKey", "HeaderValue");

            // Act
            request.SetHeader("HeaderKey", "NewHeader!");

            // Assert
            Assert.True(request.Headers["HeaderKey"] == "NewHeader!");
        }

        [Fact]
        public void SetHeader_AddIfHeaderDoesntExist()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            // Act
            request.SetHeader("HeaderKey", "HeaderValue");

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

        [Fact]
        public void RemoveParameter_RemovesAllParametersWithName()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            var paramOne = new Parameter("MyParameter", null, ParameterType.Body);
            var paramTwo = new Parameter("MyParameter", null, ParameterType.Query);
            var paramThree = new Parameter("MyOtherParameter", null, ParameterType.Query);

            request.AddParameter(paramOne);
            request.AddParameter(paramTwo);
            request.AddParameter(paramThree);

            // Act
            request.RemoveParameters(x => x.Name == "MyParameter");

            // Assert
            Assert.True(request.Parameters.Contains(paramThree) && request.Parameters.Count == 1);
        }

        [Fact]
        public void RemoveParameter_RemovesAllParametersWithType()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            var paramOne = new Parameter("MyParameter", null, ParameterType.Body);
            var paramTwo = new Parameter("MyParameter", null, ParameterType.Query);
            var paramThree = new Parameter("MyOtherParameter", null, ParameterType.Query);

            request.AddParameter(paramOne);
            request.AddParameter(paramTwo);
            request.AddParameter(paramThree);

            // Act
            request.RemoveParameters(x => x.Type == ParameterType.Query);

            // Assert
            Assert.True(request.Parameters.Contains(paramOne) && request.Parameters.Count == 1);
        }

        [Fact]
        public void RemoveParameter_RemovesAllParametersWithNameAndType()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            var paramOne = new Parameter("MyParameter", null, ParameterType.Body);
            var paramTwo = new Parameter("MyParameter", null, ParameterType.Body);
            var paramThree = new Parameter("MyParameter", null, ParameterType.Query);
            var paramFour = new Parameter("MyOtherParameter", null, ParameterType.Query);

            request.AddParameter(paramOne);
            request.AddParameter(paramTwo);
            request.AddParameter(paramThree);
            request.AddParameter(paramFour);

            // Act
            request.RemoveParameters(x => x.Name == "MyParameter" && x.Type == ParameterType.Body);

            // Assert
            Assert.True(
                request.Parameters.Contains(paramThree) && 
                request.Parameters.Contains(paramFour) && 
                request.Parameters.Count == 2);
        }
    }
}
