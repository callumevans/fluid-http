using FluidHttp.Serializers;

namespace FluidHttp
{
    public static class FluidResponseParseExtensions
    {
        public static T ParseResponse<T>(this IFluidResponse response)
        {
            return ParseResponse<T>(response, new SerializationManager());
        }

        public static T ParseResponse<T>(this IFluidResponse response, SerializationManager serialisationManager)
        {
            if (response.Headers.ContainsKey(RequestHeaders.ContentType) == false)
                return default(T);

            return serialisationManager.Deserialize<T>(
                response.Headers[RequestHeaders.ContentType], response.Content);
        }

        public static T ParseResponse<T>(this IFluidResponse response, ISerializerStrategy serialiser)
        {
            return serialiser.Deserialize<T>(response.Content);
        }

        public static T ParseResponse<T>(this SerializationManager manager, IFluidResponse response)
        {
            return manager.Deserialize<T>(response.Headers[RequestHeaders.ContentType], response.Content);
        }
    }
}
