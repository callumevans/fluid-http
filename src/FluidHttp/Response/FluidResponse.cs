using System.Collections.Generic;

namespace FluidHttp.Response
{
    public class FluidResponse
    {
        public Dictionary<string, string> Headers { get; set; }
            = new Dictionary<string, string>();

        public string Content { get; set; }
    }
}
