using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FluidHttp
{
    public class FluidRequest
    {
        private readonly IDictionary<string, string> headers = new Dictionary<string, string>();
        private readonly List<Parameter> parameters = new List<Parameter>();

        public IReadOnlyDictionary<string, string> Headers => new Dictionary<string, string>(this.headers);
        public IReadOnlyList<Parameter> Parameters => new List<Parameter>(this.parameters);

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

                            parameters.Clear();
                            parameters.AddRange(queryStringParameters);

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
            this.headers[header] = value;
        }

        public void RemoveHeader(string header)
        {
            this.headers.Remove(header);
        }

        public void AddQueryParameter(string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Query);
        }

        public void AddBodyParameter(string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Body);
        }

        public void AddParameter(Parameter parameter)
        {
            this.parameters.Add(parameter);
        }

        public void AddParameter(string parameterName, object value, ParameterType type)
        {
            AddParameter(new Parameter(parameterName, value, type));
        }

        public void RemoveParameters(Predicate<Parameter> predicate)
        {
            this.parameters.RemoveAll(predicate);
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
