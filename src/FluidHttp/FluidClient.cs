using FluidHttp.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluidHttp
{
    public class FluidClient : IFluidClient, IDisposable
    {
        private readonly ConcurrentDictionary<string, string> defaultHeaders = new ConcurrentDictionary<string, string>();
        private readonly HttpClient httpClient;
        
        private readonly string[] ReservedContentHeaders =
        {
            RequestHeaders.Allow,
            RequestHeaders.ContentDisposition,
            RequestHeaders.ContentEncoding,
            RequestHeaders.ContentLanguage,
            RequestHeaders.ContentLength,
            RequestHeaders.ContentLocation,
            RequestHeaders.ContentMD5,
            RequestHeaders.ContentRange,
            RequestHeaders.ContentType,
            RequestHeaders.Expires,
            RequestHeaders.LastModified
        };

        public string BaseUrl { get; } = string.Empty;

        public bool BaseUrlSet => !(string.IsNullOrWhiteSpace(BaseUrl));

        public FluidClient()
        {
            this.httpClient = new HttpClient();
        }
        
        public FluidClient(HttpMessageHandler messageHandler)
            : this(string.Empty, messageHandler)
        {
        }
        
        public FluidClient(string url, HttpMessageHandler messageHandler)
            : this(url)
        {
            this.httpClient = new HttpClient(messageHandler);
        }

        public FluidClient(string url)
        {            
            if (string.IsNullOrWhiteSpace(url) == false)
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute) == false)
                    throw new BadBaseUriException();
                
                this.BaseUrl = url.Trim();
            }
        }

        public void SetDefaultHeader(string name, string value)
        {
            defaultHeaders.TryAdd(name, value);
        }       
        
        public FluidClient WithDefaultHeader(string name, string value)
        {
            SetDefaultHeader(name, value);
            return this;
        }

        public async Task<IFluidResponse> FetchAsync(IFluidRequest request)
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

                string rootUrl = this.BaseUrl.TrimEnd('/', '\\', ' ');
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
            string contentTypeHeader = RequestHeaders.ContentType;
            string contentTypeValue = MimeTypes.ApplicationFormEncoded;
            
            foreach (var requestHeader in request.Headers)
            {
                if (requestHeader.Key.ToLower() == RequestHeaders.ContentType.ToLower())
                {
                    contentTypeHeader = requestHeader.Key;
                    contentTypeValue = requestHeader.Value;
                    break;
                }
            }
            
            httpRequest.Content = new StringContent(
                bodyContent,
                Encoding.UTF8,
                contentTypeValue);
            
            request.Headers.Remove(contentTypeHeader);

            // Build headers
            foreach (var header in request.Headers)
            {
                if (ReservedContentHeaders.Contains(header.Key, StringComparer.InvariantCultureIgnoreCase))
                {
                    httpRequest.Content.Headers.Add(header.Key, header.Value);
                }
                else
                {
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }

            // Execute request
            HttpResponseMessage httpResponse = await httpClient
                .SendAsync(httpRequest)
                .ConfigureAwait(false);

            var response = await BuildResponseFromHttpMessage(httpResponse)
                .ConfigureAwait(false);

            return response;
        }

        private string BuildQueryString(IEnumerable<Parameter> parameters)
        {
            var queryString = new StringBuilder();
            var lastParameter = parameters.Last();
            
            foreach (var parameter in parameters)
            {
                queryString.Append(ParameterToQueryString(parameter));

                if (parameter != lastParameter)
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

        private async Task<IFluidResponse> BuildResponseFromHttpMessage(HttpResponseMessage message)
        {
            string content;
            HttpStatusCode statusCode;
            var headers = new Dictionary<string, string>();

            content = await message.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            foreach (var header in message.Headers.Concat(message.Content.Headers))
            {
                headers.Add(header.Key, string.Join(",", header.Value));
            }

            statusCode = message.StatusCode;
            return new FluidResponse(headers, content, statusCode);
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
