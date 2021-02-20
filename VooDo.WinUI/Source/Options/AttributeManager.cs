using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Utils;
using VooDo.WinUI.Core;

namespace VooDo.WinUI.Options
{

    internal static class AttributeManager
    {

        private static ImmutableArray<Assembly> GetAssemblies()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(_a => _a == assembly || _a.GetReferencedAssemblies().Contains(assemblyName))
                .ToImmutableArray();
        }

        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(ImmutableDictionary<Type, ImmutableArray<AssemblyTagAttribute>> _map) where TAttribute : AssemblyTagAttribute
            => _map.GetValueOrDefault(typeof(TAttribute)).EmptyIfDefault().Cast<TAttribute>();

        private static BindingManagerOptions? s_cached;
        private static ImmutableArray<Assembly> s_assemblies = ImmutableArray.Create<Assembly>();

        internal static BindingManagerOptions RetrieveOptions()
        {
            ImmutableArray<Assembly> assemblies = GetAssemblies();
            if (s_cached is not null && s_assemblies.SequenceEqual(assemblies))
            {
                return s_cached;
            }
            s_assemblies = assemblies;
            // Assembly attributes
            ImmutableDictionary<Type, ImmutableArray<AssemblyTagAttribute>> assemblyMap = AssemblyTagAttribute.RetrieveAttributes(assemblies);
            ImmutableArray<Reference> references = ReferenceAttribute.Resolve(GetAttributes<ReferenceAttribute>(assemblyMap));
            ImmutableArray<Constant> constants = ConstantAttribute.Resolve(GetAttributes<ConstantAttribute>(assemblyMap));
            ImmutableArray<UsingNamespace> usingNamespaces = UsingNamespaceAttribute.Resolve(GetAttributes<UsingNamespaceAttribute>(assemblyMap));
            ImmutableArray<UnresolvedType> usingStaticTypes = UsingStaticAttribute.Resolve(GetAttributes<UsingStaticAttribute>(assemblyMap));
            // Type attributes
            ImmutableDictionary<Type, ImmutableArray<TypeTagAttribute.Application>> typeMap = TypeTagAttribute.RetrieveAttributes(assemblies);
            ITargetProvider? targetProvider = TargetProviderAttribute.Resolve(typeMap.GetValueOrDefault(typeof(TargetProviderAttribute)).EmptyIfDefault());
            targetProvider ??= BindingManagerOptions.Empty.TargetProvider;
            IHookInitializer? hookInitializer = HookInitializerAttribute.Resolve(typeMap.GetValueOrDefault(typeof(HookInitializerAttribute)).EmptyIfDefault());
            hookInitializer ??= BindingManagerOptions.Empty.HookInitializer;
            s_cached = new BindingManagerOptions(references, usingNamespaces, usingStaticTypes, constants, hookInitializer, targetProvider);
            return s_cached;
        }

    }

}
