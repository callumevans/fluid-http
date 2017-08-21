using System.Collections.Generic;

namespace FluidHttp
{
    internal static class RequestHeaders
    {
        public const string ContentType = "Content-Type";

        public static IEnumerable<string> ReservedHeaders => new List<string>(reservedHeaders);

        private static readonly IList<string> reservedHeaders =
            new List<string>
            {
                ContentType
            };
    }
}
