using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidHttp.Serialiser
{
    public class SerialisationManager
    {
        private readonly Dictionary<string, Type> serialisers;

        public SerialisationManager()
        {
            serialisers = new Dictionary<string, Type>();

            // Default serialisers
            SetSerialiser<JsonSerialisationStrategy>(MimeTypes.ApplicationJson);
            SetSerialiser<XmlSerialisationStrategy>(MimeTypes.ApplicationXml);
        }

        public void SetSerialiser<T>(string contentType)
            where T : IDeserialiserStrategy
        {
            serialisers.Add(contentType, typeof(T));
        }

        public T Deserialise<T>(string contentType, string content)
        {
            Type serialiserType = serialisers
                .Where(x => x.Key == contentType)
                .SingleOrDefault().Value;

            if (serialiserType == null)
                return default(T);

            IDeserialiserStrategy serialiser = (IDeserialiserStrategy)Activator.CreateInstance(serialiserType);

            return serialiser.Deserialise<T>(content);
        }
    }
}
