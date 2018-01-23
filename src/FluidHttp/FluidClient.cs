using FluidHttp.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluidHttp
{
    public class FluidClient : IFluidClient, IDisposable
    {
        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (Uri.IsWellFormedUriString(value, UriKind.Absolute) == false)
                    throw new BadBaseUriException();

                this.baseUrl = value.Trim();
            }
        }

        private string baseUrl;

        protected bool BaseUrlSet
        {
            get
            {
                return !(string.IsNullOrWhiteSpace(baseUrl));
            }
        }

        private readonly ConcurrentDictionary<string, string> defaultHeaders = new ConcurrentDictionary<string, string>();
        private readonly HttpClient httpClient;
        
        private readonly string[] ReservedContentHeaders =
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified"
        };

        public FluidClient()
            : this(string.Empty)
        {
        }

        public FluidClient(HttpMessageHandler messageHandler)
            : this(messageHandler, string.Empty)
        {
        }

        public FluidClient(HttpMessageHandler messageHandler, string url)
        {
            this.httpClient = new HttpClient(messageHandler);
            this.BaseUrl = url;
        }

        public FluidClient(string url)
        {
            this.httpClient = new HttpClient();
            this.BaseUrl = url;
        }

        public void SetDefaultHeader(string name, string value)
        {
            defaultHeaders.TryAdd(name, value);

            if (!this.ReservedContentHeaders.Contains(name))
            {
                this.httpClient.DefaultRequestHeaders.Add(
                    name, value);
            }
        }

        public async Task<FluidResponse> FetchAsync(IFluidRequest request)
        {
            string requestUrl = Uri.EscapeUriString(request.Url.Trim());

            if (BaseUrlSet == true)
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

            // Append any default headers
            foreach (var header in defaultHeaders)
            {
                if (request.Headers.ContainsKey(header.Key) == false)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            // Build up query string for request url
            List<Parameter> queryStringParameters = request.Parameters
                .Where(x => x.Type == ParameterType.Query)
                .ToList();

            if (queryStringParameters.Any())
            {
                requestUrl += "?" + BuildQueryString(queryStringParameters);
            }

            // Prepare request message
            var httpRequest = new HttpRequestMessage(request.Method, requestUrl);

            // Build body query sting
            string bodyContent = string.Empty;

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                List<Parameter> bodyQueryStringParameters = request.Parameters
                    .Where(x => x.Type == ParameterType.Body)
                    .ToList();

                if (bodyQueryStringParameters.Any())
                {
                    bodyContent = BuildQueryString(bodyQueryStringParameters);
                }
            }
            else
            {
                bodyContent = request.Body;
            }

            // Set content type
            string contentTypeValue;
            request.Headers.TryGetValue(RequestHeaders.ContentType, out contentTypeValue);

            httpRequest.Content = new StringContent(
                bodyContent,
                Encoding.UTF8,
                contentTypeValue ?? MimeTypes.ApplicationFormEncoded);
            
            request.Headers.Remove(RequestHeaders.ContentType);

            // Build headers
            foreach (var header in request.Headers)
            {
                if (ReservedContentHeaders.Contains(header.Key))
                {
                    httpRequest.Content.Headers.Add(header.Key, header.Value);
                }
                else
                {
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }

            // Execute request
            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            var response = await new FluidResponse()
                .FromHttpResponseMessage(httpResponse)
                .ConfigureAwait(false);

            return response;
        }

        public Task<FluidResponse> FetchAsync()
        {
            if (BaseUrlSet == false)
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

        private string BuildQueryString(IEnumerable<Parameter> parameters)
        {
            var queryString = new StringBuilder();

            foreach (var parameter in parameters)
            {
                queryString.Append(ParameterToQueryString(parameter));

                if (parameter != parameters.Last())
                    queryString.Append("&");
            }

            return Uri.EscapeUriString(queryString.ToString());
        }

        private string ParameterToQueryString(Parameter parameter)
        {
            var parameterString = new StringBuilder();

            if (parameter == null || parameter.Value == null)
            {
                parameterString.Append($"{parameter.Name}");
            }
            else if (parameter.Value is IEnumerable<object> enumerable)
            {
                foreach (var value in enumerable)
                {
                    parameterString.Append(
                        $"{parameter.Name}[]={value.ToString()}");

                    if (value != enumerable.Last())
                        parameterString.Append("&");
                }
            }
            else
            {
                parameterString.Append(
                    $"{parameter.Name}={parameter.Value.ToString()}");
            }

            return parameterString.ToString();
        }

        #region IDisposable

        protected bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FluidClient()
        {
            Dispose(false);
        }

        #endregion
    }
}
