using FluidHttp.Extensions;
using FluidHttp.Response;
using FluidHttp.Serializers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace FluidHttp.Tests
{
    public class DeserialisationTests
    {
        private static readonly string name = "Test Name";
        private static readonly int age = 24;
        private static readonly bool truthy = true;

        string jsonContent;
        string xmlContent;

        public DeserialisationTests()
        {
            // Set Json content
            jsonContent = JsonConvert.SerializeObject(new
            {
                Name = name,
                Age = age,
                Truthy = truthy
            });

            // Set XML content
            using (var stringWriter = new StringWriter())
            {
                var serialiser = new XmlSerializer(typeof(Person));

                serialiser.Serialize(stringWriter, new Person
                {
                    Name = name,
                    Age = age,
                    Truthy = truthy
                });

                xmlContent = stringWriter.ToString();
            }
        }

        [Fact]
        public void ReturnsJsonContent_DeserialiseToObject()
        {
            // Arrage
            FluidResponse response = new FluidResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Content = jsonContent
            };

            // Act
            Person person = response.ParseResponse<Person>();

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Fact]
        public void ReturnsXmlContent_DeserialiseToObject()
        {
            // Arrange
            FluidResponse response = new FluidResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/xml" }
                },
                Content = xmlContent
            };

            // Act
            Person person = response.ParseResponse<Person>();

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ReturnsContentWithoutMimeType_ReturnDefault(string contentType)
        {
            // Arrange
            FluidResponse response = new FluidResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentType }
                },
                Content = jsonContent
            };

            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithoutContentTypeHeader_ReturnDefault()
        {
            // Arrange
            FluidResponse response = new FluidResponse
            {
                Content = jsonContent
            };

            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithWrongMimeType_ReturnDefault()
        {
            // Arrange
            FluidResponse response = new FluidResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/xml" }
                },
                Content = jsonContent
            };

            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithWrongMimeType_ManualStrategy_DeserialiseSuccessfully()
        {
            // Arrange
            FluidResponse response = new FluidResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/xml" }
                },
                Content = jsonContent
            };

            // Act
            Person person = response.ParseResponse<Person>(new JsonSerializationStrategy());

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        public class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public bool Truthy { get; set; }
        }
    }
}
