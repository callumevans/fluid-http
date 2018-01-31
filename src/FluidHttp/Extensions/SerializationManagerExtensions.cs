using System;
using FluidHttp.Serializers;

namespace FluidHttp
{
    public static class SerializationManagerExtensions
    {
        public static SerializationManager SetSerializer<T>(this SerializationManager manager, string contentType)
            where T : ISerializerStrategy, new()
        {
            manager.SetSerializer(contentType, Activator.CreateInstance<T>());
            return manager;
        }
    }
}