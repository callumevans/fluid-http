using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace FluidHttp.Serializers
{
    public class XmlSerializationStrategy : ISerializerStrategy
    {
        public T Deserialize<T>(string input)
        {
            var reader = new StringReader(input);
            var serializer = new XmlSerializer(typeof(T));

            T output;

            try
            {
                output = (T)serializer.Deserialize(reader);
            }
            catch
            {
                output = default(T);
            }
            finally
            {
                reader.Dispose();
            }

            return output;
        }

        public string Serialize(object input)
        {
            if (IsAnonymousType(input.GetType()))
                throw new ArgumentException("Cannot serialize an anonymous type to XML");

            string output;

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(input.GetType());

                serializer.Serialize(writer, input);
                output = writer.ToString();
            }

            return output;
        }

        private bool IsAnonymousType(Type type)
        {
            bool isCompilerGenerated = type
                .GetTypeInfo()
                .GetCustomAttributes<CompilerGeneratedAttribute>(false)
                .Any();

            bool nameIsAnonymous = type.FullName.Contains("AnonymousType");

            return isCompilerGenerated && nameIsAnonymous;
        }
    }
}
