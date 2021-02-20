using System;
using System.IO;

using VooDo.Hooks;

namespace VooDo.Caching
{

    public sealed class HookInitializerTypeNameSerializer : ISerializer<IHookInitializer>
    {

        public static HookInitializerTypeNameSerializer Instance { get; } = new HookInitializerTypeNameSerializer();

        private HookInitializerTypeNameSerializer() { }

        public IHookInitializer Deserialize(BinaryReader _reader)
            => (IHookInitializer) Activator.CreateInstance(Type.GetType(_reader.ReadString(), true)!)!;

        public void Serialize(IHookInitializer _value, BinaryWriter _writer)
            => _writer.Write(_value.GetType().AssemblyQualifiedName!);
    }

}
