using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FluidHttp.Tests.Mocks
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage ResponseMessage { get; set; } = new HttpResponseMessage
        {
            Content = new StringContent("response content!")
        };

        public string SentMessageContent { get; private set; }
        public string ResponseMessageContent { get; private set; }

        public HttpMethod RequestMethod { get; private set; }
        public Uri RequestUrl { get; private set; }
        public HttpRequestMessage RequestMessage { get; private set; }
        public HttpRequestHeaders RequestHeaders { get; private set; }
        public HttpContentHeaders ContentHeaders { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SentMessageContent = request.Content.ReadAsStringAsync().Result;
            ResponseMessageContent = ResponseMessage.Content.ReadAsStringAsync().Result;

            RequestMethod = request.Method;
            RequestUrl = request.RequestUri;
            RequestMessage = request;
            RequestHeaders = request.Headers;
            ContentHeaders = request.Content.Headers;

            return Task.FromResult(ResponseMessage);
        }
    }
}
