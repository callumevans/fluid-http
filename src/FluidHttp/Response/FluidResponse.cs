using System.Collections.Generic;

namespace FluidHttp
{
    public class FluidResponse
    {
        public Dictionary<string, string> Headers { get; set; }
            = new Dictionary<string, string>();

        public string Content { get; set; }
    }
}
