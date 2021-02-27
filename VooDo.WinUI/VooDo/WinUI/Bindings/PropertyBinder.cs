using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Bindings
{

    internal static class PropertyBinder
    {

        internal sealed record Key(string Code, string Property, string Object);

        private static readonly LRUCache<Type, ImmutableDictionary<Key, Loader>> s_loaderCache = new(256);

        private static ImmutableDictionary<Key, Loader> GetLoaders(Type _ownerType)
        {
            if (!s_loaderCache.TryGetValue(_ownerType, out ImmutableDictionary<Key, Loader> loaders))
            {
                loaders = _ownerType
                    .GetNestedTypes(BindingFlags.NonPublic)
                    .Where(_t => _t.Name.StartsWith("VooDo_GeneratedPropertyScript_")
                        && _t.IsSubclassOf(typeof(ITypedProgram)))
                    .Select(_t => Loader.FromType(_t))
                    .ToImmutableDictionary(_t => new Key(
                        _t.GetStringTag("Code"),
                        _t.GetStringTag("TargetProperty"),
                        _t.GetStringTag("TargetObject")
                        ));
                s_loaderCache[_ownerType] = loaders;
            }
            return loaders;
        }

        internal static Loader GetLoader(Key _key, Type _ownerType)
            => GetLoaders(_ownerType)[_key];

    }

}
