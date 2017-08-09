using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluidHttp.Serializers
{
    public class SerializationManager
    {
        public static SerializationManager Serializer { get; }

        private const string jsonContentMatcher = "*/json*";
        private const string xmlContentMatcher = "*/xml*";

        private readonly ConcurrentDictionary<Regex, Lazy<ISerializerStrategy>> serializers;

        static SerializationManager()
        {
            Serializer = new SerializationManager();
        }

        public SerializationManager()
        {
            serializers = new ConcurrentDictionary<Regex, Lazy<ISerializerStrategy>>();

            // Default serialisers
            SetSerializer<JsonSerializationStrategy>(jsonContentMatcher);
            SetSerializer<XmlSerializationStrategy>(xmlContentMatcher);
        }

        public SerializationManager(IDictionary<string, ISerializerStrategy> strategies)
        {
            serializers = new ConcurrentDictionary<Regex, Lazy<ISerializerStrategy>>();

            // Configure serializers
            foreach (var strategy in strategies)
            {
                SetSerializer(strategy.Key, strategy.Value);
            }
        }

        public void SetSerializer(string contentType, ISerializerStrategy serializer)
        {
            serializers.TryAdd(BuildGlobber(contentType), new Lazy<ISerializerStrategy>(
                () => serializer));
        }

        public void SetSerializer<T>(string contentType)
            where T : ISerializerStrategy, new()
        {
            serializers.TryAdd(BuildGlobber(contentType), new Lazy<ISerializerStrategy>(
                () => new T()));
        }

        public T Deserialize<T>(string contentType, string content)
        {
            if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(content))
                return default(T);

            ISerializerStrategy strategy = GetStrategyForContentType(contentType);

            if (strategy == null)
                return default(T);

            return strategy.Deserialize<T>(content);
        }

        public ISerializerStrategy GetStrategyForContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type cannot be null or empty", contentType);

            Lazy<ISerializerStrategy> serializerDictionaryEntry = serializers
                .Where(x => x.Key.IsMatch(contentType))
                .SingleOrDefault().Value;

            if (serializerDictionaryEntry == null)
            {
                return null;
            }
            else
            {
                return serializerDictionaryEntry.Value;
            }
        }

        private Regex BuildGlobber(string pattern)
        {
            return new Regex(
                Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", "."));
        }
    }
}
