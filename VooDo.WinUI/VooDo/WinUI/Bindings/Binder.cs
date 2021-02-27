using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.Runtime.Implementation;
using VooDo.WinUI.Generator;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Bindings
{

    public static class Binder
    {

        private abstract class SpyProgram : Program
        {
            internal const string scriptPrefix = __VooDo_Reserved_scriptPrefix;
            private SpyProgram() { }
        }

        #region Class

        private static readonly LRUCache<Type, ImmutableArray<Loader>> s_classLoaderCache = new(256);

        private static ImmutableArray<Loader> GetClassLoaders(Type _ownerType)
        {
            if (!s_classLoaderCache.TryGetValue(_ownerType, out ImmutableArray<Loader> loaders))
            {
                loaders = _ownerType
                    .GetNestedTypes(BindingFlags.NonPublic)
                    .Where(_t => _t.Name.StartsWith(SpyProgram.scriptPrefix + Identifiers.classScriptPrefix)
                        && _t.BaseType == typeof(IProgram))
                    .Select(_t => Loader.FromType(_t))
                    .ToImmutableArray();
                s_classLoaderCache[_ownerType] = loaders;
            }
            return loaders;
        }

        public static ImmutableArray<ClassBinding> CreateBindings(object _owner)
            => GetClassLoaders(_owner.GetType()).Select(_l => new ClassBinding(_l.Create(), _owner)).ToImmutableArray();

        public static void Bind(object _owner)
        {
            Unbind(_owner);
            foreach (Binding b in CreateBindings(_owner))
            {
                BindingManager.Add(b);
            }
        }

        public static void Unbind(object _owner)
        {
            foreach (Binding b in BindingManager.GetByXamlOwner(_owner))
            {
                BindingManager.Remove(b);
            }
        }

        #endregion

        #region Property

        internal sealed record PropertyKey(string Code, string Property, string Object);

        private static readonly LRUCache<Type, ImmutableDictionary<PropertyKey, Loader>> s_propertyLoaderCache = new(256);

        private static ImmutableDictionary<PropertyKey, Loader> GetPropertyLoaders(Type _ownerType)
        {
            if (!s_propertyLoaderCache.TryGetValue(_ownerType, out ImmutableDictionary<PropertyKey, Loader> loaders))
            {
                loaders = _ownerType
                    .GetNestedTypes(BindingFlags.NonPublic)
                    .Where(_t => _t.Name.StartsWith(SpyProgram.scriptPrefix + Identifiers.propertyScriptPrefix)
                        && _t.IsSubclassOf(typeof(ITypedProgram)))
                    .Select(_t => Loader.FromType(_t))
                    .ToImmutableDictionary(_t => new PropertyKey(
                        _t.GetStringTag(Identifiers.propertyCodeTag),
                        _t.GetStringTag(Identifiers.propertyPropertyTag),
                        _t.GetStringTag(Identifiers.propertyObjectTag)
                        ));
                s_propertyLoaderCache[_ownerType] = loaders;
            }
            return loaders;
        }

        internal static Loader GetPropertyLoader(PropertyKey _key, Type _ownerType)
            => GetPropertyLoaders(_ownerType)[_key];


        #endregion

    }

}
