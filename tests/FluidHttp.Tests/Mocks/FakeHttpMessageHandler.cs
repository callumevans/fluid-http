using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluidHttp.Tests.Mocks
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public FakeHttpMessageHandler(string content = "response content!", string contentType = "application/json")
        {
            ResponseMessage = new HttpResponseMessage
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };
        }
        
        public HttpResponseMessage ResponseMessage { get; set; }

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
