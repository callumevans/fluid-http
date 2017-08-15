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
    }
}
