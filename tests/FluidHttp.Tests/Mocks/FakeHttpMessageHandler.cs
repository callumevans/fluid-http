using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluidHttp.Tests.Mocks
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage();

            return Task.FromResult(responseMessage);
        }
    }
}
