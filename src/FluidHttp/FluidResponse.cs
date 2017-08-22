using System.Collections.Generic;
using System.Net;

namespace FluidHttp
{
    public class FluidResponse
    {
        public Dictionary<string, string> Headers { get; set; }
            = new Dictionary<string, string>();

        public string Content { get; set; }

        public HttpStatusCode StatusCode { get; internal set; }
    }
}