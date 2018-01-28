using System.Collections.Generic;
using System.Net.Http;

namespace FluidHttp
{
    public interface IFluidRequest
    {
        IDictionary<string, string> Headers { get; set; }

        IList<Parameter> Parameters { get; set; }

        string Url { get; set; }

        string Body { get; set; }

        HttpMethod Method { get; }
    }
}
