using FluidHttp.Request;
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

        public async Task<FluidResponse> FetchAsync(FluidRequest request)
        {
            var httpRequest = new HttpRequestMessage(request.Method, request.Url);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            var response = new FluidResponse();

            response.Content = await httpResponse.Content.ReadAsStringAsync();

            return response;
        }

        public async Task<FluidResponse> FetchAsync(string url)
        {
            return await FetchAsync(url, HttpMethod.Get);
        }

        public async Task<FluidResponse> FetchAsync(string url, string method)
        {
            return await FetchAsync(url, new HttpMethod(method));
        }

        public async Task<FluidResponse> FetchAsync(string url, HttpMethod method)
        {
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.Method = method;

            return await FetchAsync(request);
        }
    }
}
