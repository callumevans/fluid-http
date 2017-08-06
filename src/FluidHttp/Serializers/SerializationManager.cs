using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidHttp.Serializers
{
    public class SerializationManager
    {
        private readonly Dictionary<string, Type> serialisers;

        public SerializationManager()
        {
            serialisers = new Dictionary<string, Type>();

            // Default serialisers
            SetSerialiser<JsonSerializationStrategy>(MimeTypes.ApplicationJson);
            SetSerialiser<XmlSerializationStrategy>(MimeTypes.ApplicationXml);
        }

        public void SetSerialiser<T>(string contentType)
            where T : IDeserializerStrategy
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

            IDeserializerStrategy serialiser = (IDeserializerStrategy)Activator.CreateInstance(serialiserType);

            return serialiser.Deserialise<T>(content);
        }
    }
}
