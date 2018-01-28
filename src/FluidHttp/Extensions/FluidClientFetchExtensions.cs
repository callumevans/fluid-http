using System.Net.Http;
using System.Threading.Tasks;
using FluidHttp.Exceptions;

namespace FluidHttp
{
    public static class FluidClientExtensions
    {
        public static Task<IFluidResponse> FetchAsync(this IFluidClient client)
        {
            if (client.BaseUrlSet == false)
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
            IFluidRequest request = new FluidRequest(url, method);
            return client.FetchAsync(request);
        }
    }
}