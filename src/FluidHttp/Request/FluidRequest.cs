﻿using FluidHttp.Parameters;
using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FluidHttp.Request
{
    public class FluidRequest
    {
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

                if (queryStringStartIndex > -1 && newUrl.Length > 1)
                {
                    string queryString = newUrl.Substring(queryStringStartIndex);

                    if (Uri.IsWellFormedUriString(queryString, UriKind.Relative))
                    {
                        QueryParamCollection collection = Flurl.Url.ParseQueryParams(queryString);

                        parameters.AddRange(
                            collection.Select(
                                x => new Parameter(x.Name, x.Value)));

                        // Remove query string from url
                        newUrl = newUrl.Substring(0, queryStringStartIndex);
                    }
                }

                this.url = newUrl.Trim();
            }
        }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        private readonly List<Parameter> parameters = new List<Parameter>();
        private string url = string.Empty;

        public void AddQueryParameter(
            string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Query);
        }

        public void AddBodyParameter(
            string parameterName, object value)
        {
            AddParameter(parameterName, value, ParameterType.Body);
        }

        public void AddParameter(
            string parameterName, object value, ParameterType type)
        {
            this.parameters.Add(new Parameter(
                parameterName, value, type));
        }
    }
}
