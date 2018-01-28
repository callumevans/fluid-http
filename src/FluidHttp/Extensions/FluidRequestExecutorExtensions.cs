using System.Threading.Tasks;

namespace FluidHttp
{
    public static class FluidRequestExecutorExtensions
    {
        private static readonly FluidClient client = new FluidClient();

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
            client.BaseUrl = baseUrl;
            return client.FetchAsync(request);
        }
        
        public static IFluidRequest WithJsonBody(this IFluidRequest request, object content)
        {
            request.Body = SerializationManager.Serializer
                .Serialize(MimeTypes.ApplicationJson, content);

            request.Headers[RequestHeaders.ContentType] = MimeTypes.ApplicationJson;
            
            return request;
        }

        public static IFluidRequest WithXmlBody(this IFluidRequest request, object content)
        {
            request.Body = SerializationManager.Serializer
                .Serialize(MimeTypes.ApplicationXml, content);
            
            request.Headers[RequestHeaders.ContentType] = MimeTypes.ApplicationXml;
            
            return request;
        }
    }
}
