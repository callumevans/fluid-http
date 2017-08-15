using FluidHttp.Serializers;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FluidHttp
{
    public class FluidRequest
    {
        public Dictionary<string, string> Headers { get; private set; }
            = new Dictionary<string, string>();

        private List<Parameter> parameters = new List<Parameter>();

        public List<Parameter> Parameters
        {
            get
            {
                return parameters;
            }
        }

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
                            parameters = ParseQueryString(queryString);

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
                SetHeader(RequestHeaders.ContentType, value);
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

        public void SetHeader(string header, string value)
        {
            this.Headers[header] = value;
        }

        public void RemoveHeader(string header)
        {
            this.Headers.Remove(header);
        }

        public void AddQueryParameter(string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Query);
        }

        public void AddBodyParameter(string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Body);
        }

        public void AddParameter(string parameterName, object value, ParameterType type)
        {
            this.parameters.Add(new Parameter(
                parameterName, value, type));
        }

        public void SetJsonBody(object content)
        {
            Body = SerializationManager.Serializer
                .Serialize(MimeTypes.ApplicationJson, content);

            SetHeader(RequestHeaders.ContentType, MimeTypes.ApplicationJson);
        }

        public void SetXmlBody(object content)
        {
            Body = SerializationManager.Serializer
                .Serialize(MimeTypes.ApplicationXml, content);

            SetHeader(RequestHeaders.ContentType, MimeTypes.ApplicationXml);
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
