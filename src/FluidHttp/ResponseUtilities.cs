using FluidHttp.Response;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FluidHttp
{
    public static class ResponseUtilities
    {
        public static T ParseResponse<T>(this FluidResponse response)
        {
            string contentType;

            if (response.Headers.ContainsKey(RequestHeaders.ContentType))
                contentType = response.Headers[RequestHeaders.ContentType];
            else
                return default(T);

            try
            {
                if (contentType == MimeTypes.ApplicationJson)
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                else if (contentType == MimeTypes.ApplicationXml)
                {
                    using (var reader = new StringReader(response.Content))
                    {
                        var serialiser = new XmlSerializer(typeof(T));
                        T output = (T)serialiser.Deserialize(reader);

                        return output;
                    }
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }

        public async static Task<FluidResponse> FromHttpResponseMessage(
            this FluidResponse response, HttpResponseMessage message)
        {
            response.Content = await message.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            foreach (var header in message.Headers.Concat(message.Content.Headers))
            {
                response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            return response;
        }
    }
}
