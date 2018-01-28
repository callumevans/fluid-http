using System.Collections.Generic;
using System.Net;

namespace FluidHttp
{
    public class FluidResponse : IFluidResponse
    {
        public IDictionary<string, string> Headers { get; }

        public string Content { get; }

        public HttpStatusCode StatusCode { get; }

        public FluidResponse(
            IDictionary<string, string> headers, 
            string content, 
            HttpStatusCode statusCode)
        {
            this.Headers = headers;
            this.Content = content;
            this.StatusCode = statusCode;
        }
    }
}