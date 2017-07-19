using System;

namespace FluidHttp.Exceptions
{
    public class BadBaseUriException : Exception
    {
        public BadBaseUriException()
            : base("Invalid Base Uri set on FluidClient instance")
        {
        }
    }
}
