using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluidHttp
{
    public static class FluidResponseMapperExtensions
    {
        public async static Task<FluidResponse> FromHttpResponseMessage(
            this FluidResponse response, HttpResponseMessage message)
        {
            response.Content = await message.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            foreach (var header in message.Headers.Concat(message.Content.Headers))
            {
                response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            response.StatusCode = message.StatusCode;

            return response;
        }
    }
}