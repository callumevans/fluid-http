using System.Net.Http;
using System.Threading.Tasks;
using FluidHttp.Exceptions;

namespace FluidHttp
{
    public static class FluidClientExtensions
    {
        public static Task<IFluidResponse> FetchAsync(this IFluidClient client)
        {
            if (string.IsNullOrWhiteSpace(client.BaseUrl))
                throw new NoUrlProvidedException();

            return client.FetchAsync("");
        }

        public static Task<IFluidResponse> FetchAsync(this IFluidClient client, string url)
        {
            return client.FetchAsync(url, HttpMethod.Get);
        }

        public static Task<IFluidResponse> FetchAsync(this IFluidClient client, string url, string method)
        {
            return client.FetchAsync(url, new HttpMethod(method));
        }

        public static Task<IFluidResponse> FetchAsync(this IFluidClient client, string url, HttpMethod method)
        {
            var request = new FluidRequest(url, method);
            return client.FetchAsync(request);
        }

        public static Task<T> FetchAsync<T>(this IFluidClient client, string url)
        {
            return client.FetchAsync<T>(url, HttpMethod.Get);
        }

        public static Task<T> FetchAsync<T>(this IFluidClient client, string url, string method)
        {
            return client.FetchAsync<T>(url, new HttpMethod(method));
        }

        public static async Task<T> FetchAsync<T>(this IFluidClient client, string url, HttpMethod method)
        {
            var request = new FluidRequest(url, method);
            IFluidResponse response = await client.FetchAsync(request);
            
            return response.ParseResponse<T>();
        }
    }
}