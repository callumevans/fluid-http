namespace FluidHttp.Serializers
{
    public interface IDeserializerStrategy
    {
        T Deserialise<T>(string content);
    }
}
