using FluidHttp.Tests.Mocks;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;
using static FluidHttp.Tests.DeserialisationTests;

namespace FluidHttp.Tests
{
    public class AddBodyTests
    {
        private readonly FakeHttpMessageHandler messageHandler = new FakeHttpMessageHandler();
        private readonly FluidClient client;

        private const string url = "http://localhost.com";

        private readonly Person bodyContent = new Person
        {
            Name = "Test name",
            Age = 123
        };

        public AddBodyTests()
        {
            client = new FluidClient(messageHandler);
        }

        [Fact]
        public async Task Fetch_SetJsonBody_SerializesContentAndSetsType()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.WithJsonBody(bodyContent);

            // Act
            await client.FetchAsync(request);

            // Assert
            string jsonContent = SerializationManager.Serializer
                .Serialize("application/json", bodyContent);

            Assert.Equal(jsonContent, messageHandler.SentMessageContent);
            Assert.Equal("application/json", messageHandler.ContentHeaders.ContentType.MediaType);
        }

        [Fact]
        public async Task Fetch_SetXmlBody_SerializesContentAndSetsType()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.WithXmlBody(bodyContent);

            // Act
            await client.FetchAsync(request);

            // Assert
            string xmlContent = SerializationManager.Serializer
                .Serialize("application/xml", bodyContent);

            Assert.Equal(xmlContent, messageHandler.SentMessageContent);
            Assert.Equal("application/xml", messageHandler.ContentHeaders.ContentType.MediaType);
        }

        [Fact]
        public void Fetch_SetXmlBody_WithAnonymousType_ThrowArgumentException()
        {
            // Arrange
            FluidRequest request = new FluidRequest();
            request.Url = url;
            
            var anonymousType = new
            {
                Name = "test"
            };
            
            // Act + Assert
            Assert.Throws<ArgumentException>(
                () => request.WithXmlBody(anonymousType));
        }

        [Fact]
        public async Task Fetch_AddJsonBody_IgnoreBodyParametersWhenBodySet()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.WithBodyParameter("Test", "TestValue");
            request.WithJsonBody(bodyContent);

            // Act
            await client.FetchAsync(request);

            // Assert
            string jsonContent = JsonConvert.SerializeObject(bodyContent);

            Assert.Equal(jsonContent, messageHandler.SentMessageContent);
            Assert.Equal("application/json", messageHandler.ContentHeaders.ContentType.MediaType);
        }

        [Fact]
        public async Task Fetch_AddJsonBodyThenClear_ClearBodyContent()
        {
            // Arrange
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.WithBodyParameter("Test", "TestValue");
            request.WithJsonBody(bodyContent);
            request.Body = string.Empty;

            // Act
            await client.FetchAsync(request);

            // Assert
            JsonConvert.SerializeObject(bodyContent);
            Assert.Equal("Test=TestValue", messageHandler.SentMessageContent);
        }
    }
}
