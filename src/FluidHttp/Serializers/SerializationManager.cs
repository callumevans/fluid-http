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
        public static SerializationManager Serializer { get; }

        private const string jsonContentMatcher = "*/json*";
        private const string xmlContentMatcher = "*/xml*";

        private static readonly Dictionary<string, Type> defaultConfigurations = new Dictionary<string, Type>
        {
            { jsonContentMatcher, typeof(JsonSerializationStrategy) },
            { xmlContentMatcher, typeof(XmlSerializationStrategy) }
        };

        private readonly ConcurrentDictionary<Regex, Lazy<ISerializerStrategy>> serializers;

        static SerializationManager()
        {
            Serializer = new SerializationManager();
        }

        public SerializationManager()
            : this(defaultConfigurations)
        {
        }

        public SerializationManager(IDictionary<string, Type> strategies)
        {
            serializers = new ConcurrentDictionary<Regex, Lazy<ISerializerStrategy>>();

            // Configure serializers
            foreach (var strategy in strategies)
            {
                SetSerializerInternal(strategy.Key, strategy.Value);
            }
        }

        public void SetSerializer<T>(string contentType)
            where T : ISerializerStrategy, new()
        {
            SetSerializerInternal(contentType, typeof(T));
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

        private void SetSerializerInternal(string contentType, Type serializer)
        {
            serializers.TryAdd(BuildGlobber(contentType), new Lazy<ISerializerStrategy>(
                () => (ISerializerStrategy)Activator.CreateInstance(serializer)));
        }
    }
}
