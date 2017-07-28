using System.Collections.Generic;

namespace FluidHttp
{
    internal static class RequestHeaders
    {
        public const string ContentType = "Content-Type";

        public static IList<string> ReservedHeaders => reservedHeaders.AsReadOnly();

        private static readonly List<string> reservedHeaders;

        static RequestHeaders()
        {
            reservedHeaders = new List<string>();

            reservedHeaders.Add(ContentType);
        }
    }
}
