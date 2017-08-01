using FluidHttp.Response;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluidHttp
{
    public static class ResponseUtilities
    {
        public static T ParseResponse<T>(this FluidResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

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

            return response;
        }
    }
}
