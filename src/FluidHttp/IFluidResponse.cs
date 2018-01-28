using System.Collections.Generic;
using System.Net;

namespace FluidHttp
{
    public interface IFluidResponse
    {
        IDictionary<string, string> Headers { get; }
        
        string Content { get; }
        
        HttpStatusCode StatusCode { get; }
    }
}