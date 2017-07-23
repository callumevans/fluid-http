﻿using FluidHttp.Exceptions;
using FluidHttp.Request;
using FluidHttp.Response;
using System;
using System.Linq;
using System.Net.Http;
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
        private string baseUrl = null;
        private bool baseUrlSet = false;

        public FluidClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<FluidResponse> FetchAsync()
        {
            if (baseUrlSet == false) throw new NoUrlProvidedException();

            return await FetchAsync("");
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
                string queryString = "?";

                foreach (var parameter in request.Parameters)
                {
                    queryString += parameter.ToString();

                    if (parameter != request.Parameters.Last())
                        queryString += "&";
                }

                requestUrl += queryString;
            }

            // Execute request

            var httpRequest = new HttpRequestMessage(request.Method, requestUrl);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            var response = new FluidResponse();

            response.Content = await httpResponse.Content.ReadAsStringAsync();

            return response;
        }

        public async Task<FluidResponse> FetchAsync(string url)
        {
            return await FetchAsync(url, HttpMethod.Get);
        }

        public async Task<FluidResponse> FetchAsync(string url, string method)
        {
            return await FetchAsync(url, new HttpMethod(method));
        }

        public async Task<FluidResponse> FetchAsync(string url, HttpMethod method)
        {
            FluidRequest request = new FluidRequest();

            request.Url = url;
            request.Method = method;

            return await FetchAsync(request);
        }
    }
}