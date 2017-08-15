using FluidHttp.Serializers;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluidHttp
{
    public static class ParseExtensions
    {
        public static T ParseResponse<T>(this FluidResponse response)
        {
            return ParseResponse<T>(response, new SerializationManager());
        }

        public static T ParseResponse<T>(this FluidResponse response, SerializationManager serialisationManager)
        {
            if (response.Headers.ContainsKey(RequestHeaders.ContentType) == false)
                return default(T);

            return serialisationManager.Deserialize<T>(
                response.Headers[RequestHeaders.ContentType], response.Content);
        }

        public static T ParseResponse<T>(this FluidResponse response, ISerializerStrategy serialiser)
        {
            return serialiser.Deserialize<T>(response.Content);
        }

        public static T ParseResponse<T>(this SerializationManager manager, FluidResponse response)
        {
            return manager.Deserialize<T>(response.Headers[RequestHeaders.ContentType], response.Content);
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
