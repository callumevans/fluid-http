using System.Threading.Tasks;

namespace FluidHttp
{
    public static class FluidRequestExecutorExtensions
    {
        public static IFluidRequest WithHeader(this IFluidRequest request, string header, string value)
        {
            request.Headers[header] = value;
            return request;
        }

        public static IFluidRequest WithBodyParameter(this IFluidRequest request, string name, object value)
        {
            request.Parameters.Add(new Parameter(
                name, value, ParameterType.Body));

            return request;
        }

        public static IFluidRequest WithQueryParameter(this IFluidRequest request, string name, object value)
        {
            request.Parameters.Add(new Parameter(
                name, value, ParameterType.Query));

            return request;
        }

        public static Task<FluidResponse> FetchAsync(this IFluidRequest request)
        {
            return FetchAsync(request, null);
        }

        public static Task<FluidResponse> FetchAsync(this IFluidRequest request, string baseUrl)
        {
            var client = new FluidClient(baseUrl);

            return client.FetchAsync(request);
        }
    }
}
