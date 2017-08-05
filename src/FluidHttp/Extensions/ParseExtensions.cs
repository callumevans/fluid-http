using FluidHttp.Response;
using FluidHttp.Serialiser;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluidHttp.Extensions
{
    public static class ParseExtensions
    {
        public static T ParseResponse<T>(this FluidResponse response)
        {
            return ParseResponse<T>(response, new SerialisationManager());
        }

        public static T ParseResponse<T>(this FluidResponse response, SerialisationManager serialisationManager)
        {
            if (response.Headers.ContainsKey(RequestHeaders.ContentType) == false)
                return default(T);

            return serialisationManager.Deserialise<T>(
                response.Headers[RequestHeaders.ContentType], response.Content);
        }

        public static T ParseResponse<T>(this FluidResponse response, IDeserialiserStrategy serialiser)
        {
            return serialiser.Deserialise<T>(response.Content);
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
