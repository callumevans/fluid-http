using Newtonsoft.Json;

namespace FluidHttp.Serialiser
{
    public class JsonSerialisationStrategy : IDeserialiserStrategy
    {
        public T Deserialise<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
