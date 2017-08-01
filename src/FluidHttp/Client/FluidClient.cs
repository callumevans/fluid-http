﻿using FluidHttp.Exceptions;
using FluidHttp.Request;
using FluidHttp.Response;
using System;
using System.Collections.Generic;
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
            string requestUrl = Uri.EscapeUriString(request.Url.Trim());

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

            List<Parameter> bodyQueryStringParameters = request.Parameters
                .Where(x => x.Type == ParameterType.Body)
                .ToList();

            if (bodyQueryStringParameters.Any())
            {
                bodyContent = BuildQueryString(bodyQueryStringParameters);
            }

            // Set content type
            string contentTypeValue;
            request.Headers.TryGetValue(RequestHeaders.ContentType, out contentTypeValue);

            httpRequest.Content = new StringContent(bodyContent, Encoding.UTF8, contentTypeValue ?? MimeTypes.ApplicationFormEncoded);

            request.Headers.Remove(RequestHeaders.ContentType);

            // Build headers
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
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
    }
}
