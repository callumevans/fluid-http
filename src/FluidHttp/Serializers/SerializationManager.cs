using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluidHttp.Serializers
{
    public class SerializationManager
    {
        private const string jsonContentMatcher = "*/json*";
        private const string xmlContentMatcher = "*/xml*";

        private readonly ConcurrentDictionary<string, Lazy<IDeserializerStrategy>> serializers;

        public SerializationManager()
        {
            serializers = new ConcurrentDictionary<string, Lazy<IDeserializerStrategy>>();

            // Default serialisers
            SetSerializer<JsonSerializationStrategy>(jsonContentMatcher);
            SetSerializer<XmlSerializationStrategy>(xmlContentMatcher);
        }

        public SerializationManager(IDictionary<string, IDeserializerStrategy> strategies)
        {
            serializers = new ConcurrentDictionary<string, Lazy<IDeserializerStrategy>>();

            // Configure serializers
            foreach (var strategy in strategies)
            {
                SetSerializer(strategy.Key, strategy.Value);
            }
        }

        public void SetSerializer(string contentType, IDeserializerStrategy serializer)
        {
            serializers.TryAdd(contentType, new Lazy<IDeserializerStrategy>(
                () => serializer));
        }

        public void SetSerializer<T>(string contentType)
            where T : IDeserializerStrategy, new()
        {
            serializers.TryAdd(contentType, new Lazy<IDeserializerStrategy>(
                () => new T()));
        }

        public T Deserialize<T>(string contentType, string content)
        {
            if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(content))
                return default(T);

            Lazy<IDeserializerStrategy> serializerDictionaryEntry = serializers
                .Where(x => BuildGlobber(x.Key).IsMatch(contentType))
                .SingleOrDefault().Value;

            if (serializerDictionaryEntry == null)
                return default(T);

            IDeserializerStrategy strategy = serializerDictionaryEntry.Value;

            return strategy.Deserialise<T>(content);

            Regex BuildGlobber(string pattern)
            {
                return new Regex(
                    Regex.Escape(pattern)
                    .Replace(@"\*", ".*")
                    .Replace(@"\?", "."));
            }
        }
    }
}
