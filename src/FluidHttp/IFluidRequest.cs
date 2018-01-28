using System.Collections.Generic;
using System.Net.Http;

namespace FluidHttp
{
    public interface IFluidRequest
    {
        IDictionary<string, string> Headers { get; }

        IList<Parameter> Parameters { get; }

        string Url { get; }

        string Body { get; }

        HttpMethod Method { get; set; }

        void SetBody(object content, string contentType);
    }
}
