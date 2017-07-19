using System.Net.Http;

namespace FluidHttp.Request
{
    public class FluidRequest
    {
        public string Url { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public FluidRequest()
        {

        }
    }
}
