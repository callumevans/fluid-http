namespace FluidHttp.Serializers
{
    public interface IDeserializer
    {
        T Deserialize<T>(string input);
    }
}
