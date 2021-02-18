using System;

using VooDo.Hooks;

namespace VooDo.Caching
{

    public sealed class HookInitializerProviderTypeNameSerializer : ISerializer<IHookInitializerProvider>
    {

        public static HookInitializerProviderTypeNameSerializer Instance { get; } = new HookInitializerProviderTypeNameSerializer();

        private HookInitializerProviderTypeNameSerializer() { }

        public IHookInitializerProvider Deserialize(string _serializedValue)
            => (IHookInitializerProvider) Activator.CreateInstance(Type.GetType(_serializedValue, true));

        public string Serialize(IHookInitializerProvider _value)
            => _value.GetType().AssemblyQualifiedName;

    }

}
