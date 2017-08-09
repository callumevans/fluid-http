using Newtonsoft.Json;

namespace FluidHttp.Serializers
{
    public class JsonSerializationStrategy : ISerializerStrategy
    {
        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }

        public string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }
}
