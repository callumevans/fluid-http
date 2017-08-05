namespace FluidHttp.Serialiser
{
    public interface IDeserialiserStrategy
    {
        T Deserialise<T>(string content);
    }
}
