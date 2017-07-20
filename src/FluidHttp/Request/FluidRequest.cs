using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FluidHttp.Request
{
    public class FluidRequest
    {
        public IReadOnlyList<Parameter> Parameters
        {
            get
            {
                return parameters.AsReadOnly();
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
                if (value == null)
                    value = string.Empty;

                // Parse query parameters
                int queryStringStartIndex = value.IndexOf('?');

                if (queryStringStartIndex > -1 && value.Length > 1)
                {
                    string queryString = value.Substring(queryStringStartIndex);

                    if (Uri.IsWellFormedUriString(queryString, UriKind.Relative))
                    {
                        QueryParamCollection collection = Flurl.Url.ParseQueryParams(queryString);

                        parameters.AddRange(
                            collection.Select(
                                x => new Parameter(x.Name, x.Value)));

                        // Remove query string from url
                        value = value.Substring(0, queryStringStartIndex);
                    }
                }

                this.url = value.Trim();
            }
        }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        private List<Parameter> parameters = new List<Parameter>();
        private string url = string.Empty;

        public void AddQueryParameter(string parameterName, object value)
        {
            this.parameters.Add(new Parameter(
                parameterName, value));
        }
    }
}
