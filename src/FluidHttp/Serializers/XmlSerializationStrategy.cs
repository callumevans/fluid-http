﻿using System.IO;
using System.Xml.Serialization;

namespace FluidHttp.Serializers
{
    public class XmlSerializationStrategy : IDeserializerStrategy
    {
        public T Deserialise<T>(string content)
        {
            var reader = new StringReader(content);
            var serialiser = new XmlSerializer(typeof(T));

            T output;

            try
            {
                output = (T)serialiser.Deserialize(reader);
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
    }
}