using FluidHttp.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;
using Xunit;

namespace FluidHttp.Tests
{
    public class DeserialisationTests
    {
        private const string name = "Test Name";
        private const int age = 24;
        private const bool truthy = true;

        private readonly IDictionary<string, string> jsonHeaders =
            new Dictionary<string, string>
        {
            { "Content-Type", "application/json"}
        };
    
        private readonly IDictionary<string, string> xmlHeaders =
            new Dictionary<string, string>
        {
            { "Content-Type", "application/xml"}
        };

        private readonly string jsonContent;
        private readonly string xmlContent;

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
            FluidResponse response = new FluidResponse(
                jsonHeaders, jsonContent, HttpStatusCode.OK);
            
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
            FluidResponse response = new FluidResponse(
                xmlHeaders, xmlContent, HttpStatusCode.OK);
            
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
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, xmlContent, HttpStatusCode.OK);
            
            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithoutContentTypeHeader_ReturnDefault()
        {
            // Arrange
            var noHeaders = new Dictionary<string, string>();
            
            FluidResponse response = new FluidResponse(
                noHeaders, xmlContent, HttpStatusCode.OK);
            
            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithWrongMimeType_ReturnDefault()
        {
            // Arrange
            FluidResponse response = new FluidResponse(
                xmlHeaders, jsonContent, HttpStatusCode.OK);

            // Act
            var output = response.ParseResponse<Person>();

            // Assert
            Assert.Null(output);
        }

        [Fact]
        public void ReturnsContentWithWrongMimeType_ManualStrategy_DeserialiseSuccessfully()
        {
            // Arrange
            FluidResponse response = new FluidResponse(
                xmlHeaders, jsonContent, HttpStatusCode.OK);

            // Act
            Person person = response.ParseResponse<Person>(new JsonSerializationStrategy());

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Fact]
        public void DeserializeWithSerializationManagerExtension()
        {
            // Arrange
            SerializationManager manager = new SerializationManager();

            FluidResponse response = new FluidResponse(
                jsonHeaders, jsonContent, HttpStatusCode.OK);

            // Act
            Person person = manager.ParseResponse<Person>(response);

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Theory]
        [InlineData("application/json", "*/json")]
        [InlineData("application/json", "*json*")]
        [InlineData("application/json, content-length 1000", "application/json*")]
        [InlineData("text/json", "*/json")]
        [InlineData("text/json", "*json*")]
        [InlineData("text/json, content-length 1000", "text/json*")]
        public void Deserialize_FuzzyJsonHeaders_SuccessfullyDeserialize(string contentType, string fuzzyMatcher)
        {
            // Arrange
            SerializationManager manager = new SerializationManager(
                new Dictionary<string, ISerializerStrategy>());

            manager.SetSerializer<JsonSerializationStrategy>(fuzzyMatcher);
            
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, jsonContent, HttpStatusCode.OK);

            // Act
            Person person = manager.ParseResponse<Person>(response);

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Theory]
        [InlineData("application/xml", "*/xml")]
        [InlineData("application/xml", "*xml*")]
        [InlineData("application/xml, content-length 1000", "application/xml*")]
        [InlineData("text/xml", "*/xml")]
        [InlineData("text/xml", "*xml*")]
        [InlineData("text/xml, content-length 1000", "text/xml*")]
        public void Deserialize_FuzzyXmlHeaders_SuccessfullyDeserialize(string contentType, string fuzzyMatcher)
        {
            // Arrange
            SerializationManager manager = new SerializationManager(
                new Dictionary<string, ISerializerStrategy>());

            manager.SetSerializer<XmlSerializationStrategy>(fuzzyMatcher);
            
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, xmlContent, HttpStatusCode.OK);

            // Act
            Person person = manager.ParseResponse<Person>(response);

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }
        
        [Theory]
        [InlineData("application/json", "*/json")]
        [InlineData("application/json", "*json*")]
        [InlineData("application/json, content-length 1000", "application/json*")]
        [InlineData("text/json", "*/json")]
        [InlineData("text/json", "*json*")]
        [InlineData("text/json, content-length 1000", "text/json*")]
        public void Deserialize_FuzzyJsonHeaders_PassInSerialisationStrategyInstance_SuccessfullyDeserialize(string contentType, string fuzzyMatcher)
        {
            // Arrange
            SerializationManager manager = new SerializationManager(
                new Dictionary<string, ISerializerStrategy>());

            manager.SetSerializer(fuzzyMatcher, new JsonSerializationStrategy());
            
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, jsonContent, HttpStatusCode.OK);

            // Act
            Person person = manager.ParseResponse<Person>(response);

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }
        
        [Theory]
        [InlineData("application/xml", "*/xml")]
        [InlineData("application/xml", "*xml*")]
        [InlineData("application/xml, content-length 1000", "application/xml*")]
        [InlineData("text/xml", "*/xml")]
        [InlineData("text/xml", "*xml*")]
        [InlineData("text/xml, content-length 1000", "text/xml*")]
        public void Deserialize_FuzzyXmlHeaders_PassInSerialisationStrategyInstance_SuccessfullyDeserialize(
            string contentType, string fuzzyMatcher)
        {
            // Arrange
            SerializationManager manager = new SerializationManager(
                new Dictionary<string, ISerializerStrategy>());

            manager.SetSerializer(fuzzyMatcher, new XmlSerializationStrategy());
            
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, xmlContent, HttpStatusCode.OK);

            // Act
            Person person = manager.ParseResponse<Person>(response);

            // Assert
            Assert.Equal(name, person.Name);
            Assert.Equal(age, person.Age);
            Assert.True(person.Truthy);
        }

        [Theory]
        [InlineData("application/json", "*/json", true)]
        [InlineData("application/json", "*json*", true)]
        [InlineData("application/json, content-length 1000", "application/json*", true)]
        [InlineData("text/json", "*/json", true)]
        [InlineData("text/json", "*json*", true)]
        [InlineData("text/json, content-length 1000", "text/json*", true)]
        [InlineData("application/xml", "*/xml", false)]
        [InlineData("application/xml", "*xml*", false)]
        [InlineData("application/xml, content-length 1000", "application/xml*", false)]
        [InlineData("text/xml", "*/xml", false)]
        [InlineData("text/xml", "*xml*", false)]
        [InlineData("text/xml, content-length 1000", "text/xml*", false)]
        public void Deserialize_FuzzyXmlAndJsonHeaders_SuccessfullyDeserializeWithoutConfiguration(
            string contentType, string fuzzyMatcher, bool isJson)
        {
            // Arrange
            SerializationManager manager = new SerializationManager();
            
            var testHeaders = new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            };
            
            FluidResponse response = new FluidResponse(
                testHeaders, isJson ? jsonContent : xmlContent, HttpStatusCode.OK);
            
            // Act
            Person person = manager.ParseResponse<Person>(response);

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
