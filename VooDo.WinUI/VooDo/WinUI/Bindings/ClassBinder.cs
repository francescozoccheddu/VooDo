using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Bindings
{

    public static class ClassBinder
    {

        private static readonly LRUCache<Type, ImmutableArray<Loader>> s_loaderCache = new(256);

        private static ImmutableArray<Loader> GetLoaders(Type _ownerType)
        {
            if (!s_loaderCache.TryGetValue(_ownerType, out ImmutableArray<Loader> loaders))
            {
                loaders = _ownerType
                    .GetNestedTypes(BindingFlags.NonPublic)
                    .Where(_t => _t.Name.StartsWith("VooDo_GeneratedScript_")
                        && _t.BaseType == typeof(Program))
                    .Select(_t => Loader.FromType(_t))
                    .ToImmutableArray();
                s_loaderCache[_ownerType] = loaders;
            }
            return loaders;
        }

        public static ImmutableArray<ClassBinding> CreateBindings(object _owner)
            => GetLoaders(_owner.GetType()).Select(_l => new ClassBinding(_l.Create(), _owner)).ToImmutableArray();

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

    }

}
