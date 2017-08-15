using System.Threading.Tasks;

namespace FluidHttp
{
    public static class FluidRequestExecutorExtensions
    {
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
