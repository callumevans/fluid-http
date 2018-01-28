using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FluidHttp
{
    public class FluidRequest : IFluidRequest
    {
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public IList<Parameter> Parameters { get; private set; } = new List<Parameter>();

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                string newUrl;

                if (value == null)
                    newUrl = string.Empty;
                else
                    newUrl = value;

                // Parse query parameters
                int queryStringStartIndex = newUrl.IndexOf('?');

                if (queryStringStartIndex > -1)
                {
                    string queryString = newUrl.Substring(
                        queryStringStartIndex, newUrl.Length - queryStringStartIndex).Trim('?');

                    if (string.IsNullOrWhiteSpace(queryString) == false)
                    {
                        if (Uri.IsWellFormedUriString(queryString, UriKind.Relative))
                        {
                            List<Parameter> queryStringParameters = ParseQueryString(queryString);
                            Parameters = queryStringParameters;

                            // Remove query string from url
                            newUrl = newUrl.Substring(0, newUrl.IndexOf('?'));
                        }
                    }
                }

                this.url = newUrl.Trim();
            }
        }

        private string url = string.Empty;

        public string ContentType
        {
            set
            {
                Headers[RequestHeaders.ContentType] = value;
            }
            get
            {
                if (Headers.ContainsKey(RequestHeaders.ContentType))
                {
                    return Headers[RequestHeaders.ContentType];
                }
                else
                {
                    return MimeTypes.ApplicationFormEncoded;
                }
            }
        }

        public string Body { get; set; } = string.Empty;

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public FluidRequest()
        {
        }

        public FluidRequest(string url)
        {
            this.Url = url;
        }

        public FluidRequest(HttpMethod method)
        {
            this.Method = method;
        }

        public void AddParameter(Parameter parameter)
        {
            Parameters.Add(parameter);
        }

        public void AddParameter(string parameterName, object value, ParameterType type)
        {
            AddParameter(new Parameter(parameterName, value, type));
        }

        public void SetBody(object content, string contentType)
        {
            Body = SerializationManager.Serializer
                .Serialize(contentType, content);

            Headers[RequestHeaders.ContentType] = contentType;
        }
       
        private List<Parameter> ParseQueryString(string queryString)
        {
            List<Parameter> result = new List<Parameter>();

            foreach (string keyValuePair in queryString.Split('&'))
            {
                string name;
                string value = string.Empty;

                string[] pair = keyValuePair.Split('=');

                name = pair[0];

                if (pair.Length == 2)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                result.Add(new Parameter(name, value));
            }

            return result;
        }
    }
}
