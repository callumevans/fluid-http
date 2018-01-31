using System.Net.Http;
using FluidHttp.Tests.Mocks;
using FluidHttp.Tests.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace FluidHttp.Tests
{
    public class FetchAndParseTests
    {
        private readonly string PersonJson = JsonConvert.SerializeObject(Person.TestPerson);
        
        private readonly FakeHttpMessageHandler messageHandler;
        private readonly FluidClient client;

        private const string url = "http://localhost.com";

        public FetchAndParseTests()
        {
            messageHandler = new FakeHttpMessageHandler(PersonJson);
            client = new FluidClient(messageHandler);
        }
       
        [Fact]
        public async Task FetchAndParse_ParsesResponse()
        {
            // Act
            var result = await client.FetchAsync<Person>(url);

            // Assert
            Assert.Equal(Person.TestPerson.Name, result.Name);
            Assert.Equal(Person.TestPerson.Age, result.Age);

            for (var i = 0; i < result.Cars.Count; i++)
            {
                Assert.Equal(Person.TestPerson.Cars[i].Make, result.Cars[i].Make);
                Assert.Equal(Person.TestPerson.Cars[i].Cost, result.Cars[i].Cost);
            }
        }       
               
        [Fact]
        public async Task FetchAndParse_SendsRequest()
        {
            // Act
            await client.FetchAsync<Person>(url);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(messageHandler.ResponseMessageContent));
        }   
        
        [Fact]
        public async Task FetchAndParse_WithMethodString_SendsRequest()
        {
            // Act
            string method = "GET";
            await client.FetchAsync<Person>(url, "GET");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(messageHandler.ResponseMessageContent));
            Assert.Equal(method, messageHandler.RequestMethod.Method);
        }    
        
        [Fact]
        public async Task FetchAndParse_WithMethod_SendsRequest()
        {
            // Act
            var method = HttpMethod.Post;
            await client.FetchAsync<Person>(url, method);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(messageHandler.ResponseMessageContent));
            Assert.Equal(method, messageHandler.RequestMethod);
        }
    }
}