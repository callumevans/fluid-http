using System.Threading.Tasks;

namespace FluidHttp
{
    public static class FluidRequestExecutorExtensions
    {
        public static FluidRequest WithHeader(this FluidRequest request, string header, string value)
        {
            request.SetHeader(header, value);
            return request;
        }

        public static FluidRequest WithBodyParameter(this FluidRequest request, string name, object value)
        {
            request.AddBodyParameter(name, value);
            return request;
        }

        public static FluidRequest WithQueryParameter(this FluidRequest request, string name, object value)
        {
            request.AddQueryParameter(name, value);
            return request;
        }

        public static Task<FluidResponse> FetchAsync(this FluidRequest request)
        {
            return FetchAsync(request, null);
        }

        public static Task<FluidResponse> FetchAsync(this FluidRequest request, string baseUrl)
        {
            var client = new FluidClient(baseUrl);

            return client.FetchAsync(request);
        }
    }
}
