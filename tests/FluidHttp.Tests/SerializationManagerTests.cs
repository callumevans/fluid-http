using FluidHttp.Serializers;
using System;
using Xunit;

namespace FluidHttp.Tests
{
    public class SerializationManagerTests
    {
        private readonly SerializationManager manager;
        
        public SerializationManagerTests()
        {
            manager = new SerializationManager();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GetSerializerStrategyForContentType_NoContentType_ThrowsArgumentException(string contentType)
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(
                () => manager.GetStrategyForContentType(contentType));
        }
    }
}
