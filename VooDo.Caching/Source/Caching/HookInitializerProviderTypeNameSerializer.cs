using System;
using System.IO;

using VooDo.Hooks;

namespace VooDo.Caching
{

    public sealed class HookInitializerProviderTypeNameSerializer : ISerializer<IHookInitializerProvider>
    {

        public static HookInitializerProviderTypeNameSerializer Instance { get; } = new HookInitializerProviderTypeNameSerializer();

        private HookInitializerProviderTypeNameSerializer() { }

        public IHookInitializerProvider Deserialize(BinaryReader _reader)
            => (IHookInitializerProvider) Activator.CreateInstance(Type.GetType(_reader.ReadString(), true));

        public void Serialize(IHookInitializerProvider _value, BinaryWriter _writer)
            => _writer.Write(_value.GetType().AssemblyQualifiedName);

    }

}
