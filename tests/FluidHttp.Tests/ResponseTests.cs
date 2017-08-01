using FluidHttp.Response;
using Newtonsoft.Json;
using Xunit;

namespace FluidHttp.Tests
{
    public class ResponseTests
    {
        [Fact]
        public void ReturnsJsonContent_DeserialiseToObject()
        {
            // Arrage
            string content = JsonConvert.SerializeObject(new
            {
                Name = "Test Name",
                Age = 24,
                IsBoolable = true
            });

            FluidResponse response = new FluidResponse
            {
                Content = content
            };

            // Act
            Person person = response.ParseResponse<Person>();

            // Assert
            Assert.Equal("Test Name", person.Name);
            Assert.Equal(24, person.Age);
            Assert.True(person.IsBoolable);
        }

        [Fact]
        public void NoResponseContentType_DeserialiseToObject()
        {
            // Arrage
            string content = JsonConvert.SerializeObject(new
            {
                Name = "Test Name",
                Age = 24,
                IsBoolable = true
            });

            FluidResponse response = new FluidResponse
            {
                Content = content
            };

            // Act
            Person person = response.ParseResponse<Person>();

            // Assert
            Assert.Equal("Test Name", person.Name);
            Assert.Equal(24, person.Age);
            Assert.True(person.IsBoolable);
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public bool IsBoolable { get; set; }
        }
    }
}
