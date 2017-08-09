namespace FluidHttp.Serializers
{
    public interface ISerializerStrategy
    {
        T Deserialize<T>(string input);

        string Serialize(object input);
    }
}
