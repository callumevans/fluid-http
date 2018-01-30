using FluidHttp.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluidHttp
{
    public class SerializationManager
    {
        private const string jsonContentMatcher = "*/json*";
        private const string xmlContentMatcher = "*/xml*";

        private static readonly Dictionary<string, ISerializerStrategy> defaultConfigurations = 
            new Dictionary<string, ISerializerStrategy>
        {
            { jsonContentMatcher, new JsonSerializationStrategy() },
            { xmlContentMatcher, new XmlSerializationStrategy() }
        };
        
        public static readonly SerializationManager Serializer =
            new SerializationManager(defaultConfigurations);
        
        private readonly ConcurrentDictionary<Regex, ISerializerStrategy> serializers;

        public SerializationManager()
            : this(defaultConfigurations)
        {
        }

        public SerializationManager(IDictionary<string, ISerializerStrategy> strategies)
        {
            serializers = new ConcurrentDictionary<Regex, ISerializerStrategy>();

            // Configure serializers
            foreach (var strategy in strategies)
            {
                SetSerializerInternal(strategy.Key, strategy.Value);
            }
        }

        public void SetSerializer<T>(string contentType)
            where T : ISerializerStrategy, new()
        {
            SetSerializerInternal(contentType, Activator.CreateInstance<T>());
        }

        public void SetSerializer(string contentType, ISerializerStrategy serializer)
        {
            SetSerializerInternal(contentType, serializer);
        }

        public T Deserialize<T>(string contentType, string content)
        {
            if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(content))
                return default(T);

            IDeserializer strategy = GetStrategyForContentType(contentType);

            if (strategy == null)
                return default(T);

            return strategy.Deserialize<T>(content);
        }

        public string Serialize(string contentType, object content)
        {
            ISerializer strategy = GetStrategyForContentType(contentType);
            return strategy.Serialize(content);
        }

        private ISerializerStrategy GetStrategyForContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type cannot be null or empty", contentType);

            ISerializerStrategy serializerDictionaryEntry = serializers
                .First(x => x.Key.IsMatch(contentType)).Value;

            return serializerDictionaryEntry;
        }

        private Regex BuildGlobber(string pattern)
        {
            return new Regex(
                Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", "."));
        }

        private void SetSerializerInternal(string contentType, ISerializerStrategy serializer)
        {
            serializers.TryAdd(BuildGlobber(contentType), serializer);
        }
    }
}
