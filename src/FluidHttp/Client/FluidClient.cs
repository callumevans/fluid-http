using FluidHttp.Exceptions;
using FluidHttp.Request;
using FluidHttp.Response;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluidHttp.Client
{
    public class FluidClient
    {
        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                baseUrlSet = !(string.IsNullOrWhiteSpace(value));

                if (baseUrlSet == false) return;

                if (Uri.IsWellFormedUriString(value, UriKind.Absolute) == false)
                    throw new BadBaseUriException();

                this.baseUrl = value.Trim();
            }
        }

        private readonly HttpClient httpClient;

        private string baseUrl;
        private bool baseUrlSet;

        public FluidClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<FluidResponse> FetchAsync(FluidRequest request)
        {
            string requestUrl = Uri.EscapeUriString(
                request.Url.Trim());

            if (baseUrlSet == true)
            {
                // Make sure resource url is a valid relative uri
                // so we can safely append it to the client's BaseUrl

                string resourceUrl = requestUrl;

                if (Uri.IsWellFormedUriString(resourceUrl, UriKind.Relative) == false)
                    throw new BadRelativeUriException();

                // Safely combine the base url with the resource url

                string rootUrl = this.baseUrl.TrimEnd('/', '\\', ' ');
                resourceUrl = resourceUrl.TrimStart('/', '\\', ' ');

                requestUrl = $"{rootUrl}/{resourceUrl}";
            }
            else
            {
                // If a BaseUrl has not been set then treat the resource url 
                // as the base and make sure it's a valid absolute uri

                if (Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute) == false)
                    throw new BadAbsoluteUriException();
            }

            // Build up query string for request url
            if (request.Parameters.Count > 0)
            {
                var queryString = new StringBuilder();

                queryString.Append("?");

                foreach (var parameter in request.Parameters)
                {
                    queryString.Append(parameter.ToString());

                    if (parameter != request.Parameters.Last())
                        queryString.Append("&");
                }

                requestUrl += queryString;
            }

            // Execute request
            var httpRequest = new HttpRequestMessage(request.Method, requestUrl);
            httpRequest.Content = new StringContent("TestValue=test+value");

            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            var response = new FluidResponse();

            response.Content = await httpResponse.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            return response;
        }

        public Task<FluidResponse> FetchAsync()
        {
            if (baseUrlSet == false)
                throw new NoUrlProvidedException();

            return FetchAsync("");
        }

        public Task<FluidResponse> FetchAsync(string url)
        {
            return FetchAsync(url, HttpMethod.Get);
        }

        public Task<FluidResponse> FetchAsync(string url, string method)
        {
            return FetchAsync(url, new HttpMethod(method));
        }

        public Task<FluidResponse> FetchAsync(string url, HttpMethod method)
        {
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.Method = method;

            return FetchAsync(request);
        }
    }
}
