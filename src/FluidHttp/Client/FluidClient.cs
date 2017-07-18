using FluidHttp.Response;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluidHttp.Client
{
    public class FluidClient
    {
        private HttpClient httpClient;

        public FluidClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<FluidResponse> Fetch(string url, string method)
        {
            return await Fetch(url, new HttpMethod(method));
        }

        public async Task<FluidResponse> Fetch(string url)
        {
            return await Fetch(url, HttpMethod.Get);
        }

        public async Task<FluidResponse> Fetch(string url, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, url);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(request);

            var response = new FluidResponse();

            response.Content = await httpResponse.Content.ReadAsStringAsync();

            return response;
        }
    }
}
