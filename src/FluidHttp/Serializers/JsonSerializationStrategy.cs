using Newtonsoft.Json;

namespace FluidHttp.Serializers
{
    public class JsonSerializationStrategy : IDeserializerStrategy
    {
        public T Deserialise<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
