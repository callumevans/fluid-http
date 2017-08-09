using System.IO;
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
            throw new System.NotImplementedException();
        }
    }
}
