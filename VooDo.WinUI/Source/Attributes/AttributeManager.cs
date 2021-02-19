using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Utils;
using VooDo.WinUI.Core;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Attributes
{

    public static class AttributeManager
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

        public static void Update()
        {
            ImmutableArray<Assembly> assemblies = GetAssemblies();
            if (s_initialized && s_lastAssemblies.SequenceEqual(assemblies))
            {
                return;
            }
            s_initialized = true;
            s_lastAssemblies = assemblies;
            {
                // Assembly attributes
                ImmutableDictionary<Type, ImmutableArray<AssemblyTagAttribute>> assemblyMap = AssemblyTagAttribute.RetrieveAttributes(assemblies);
                s_references = ReferenceAttribute.Resolve(assemblyMap.GetValueOrDefault(typeof(ReferenceAttribute)).EmptyIfDefault().Cast<ReferenceAttribute>());
                s_usingStaticTypes = UsingStaticAttribute.Resolve(assemblyMap.GetValueOrDefault(typeof(UsingStaticAttribute)).EmptyIfDefault().Cast<UsingStaticAttribute>());
                s_usingNamespaceDirectives = UsingNamespaceAttribute.Resolve(assemblyMap.GetValueOrDefault(typeof(UsingNamespaceAttribute)).EmptyIfDefault().Cast<UsingNamespaceAttribute>());
            }
            {
                // Type attributes
                ImmutableDictionary<Type, ImmutableArray<TypeTagAttribute.Application>> typeMap = TypeTagAttribute.RetrieveAttributes(assemblies);
                s_bindingManager = BindingManagerAttribute.Resolve(typeMap.GetValueOrDefault(typeof(BindingManagerAttribute)).EmptyIfDefault());
                s_targetProvider = TargetProviderAttribute.Resolve<ITarget>(typeMap.GetValueOrDefault(typeof(TargetProviderAttribute)).EmptyIfDefault());
                s_loaderProvider = LoaderProviderAttribute.Resolve<ITarget>(typeMap.GetValueOrDefault(typeof(LoaderProviderAttribute)).EmptyIfDefault());
                s_hookInitializerProvider = HookInitializerProviderAttribute.Resolve(typeMap.GetValueOrDefault(typeof(HookInitializerProviderAttribute)).EmptyIfDefault());
            }
            LoaderOptions.Update();
        }

        private static void EnsureInitialized()
        {
            if (!s_initialized)
            {
                Update();
            }
        }

        private static ImmutableArray<Assembly> s_lastAssemblies;
        private static bool s_initialized;
        private static IBindingManager? s_bindingManager;
        private static ITargetProvider<ITarget>? s_targetProvider;
        private static ILoaderProvider<ITarget>? s_loaderProvider;
        private static IHookInitializerProvider? s_hookInitializerProvider;
        private static ImmutableArray<Reference> s_references = ImmutableArray.Create<Reference>();
        private static ImmutableArray<Type> s_usingStaticTypes = ImmutableArray.Create<Type>();
        private static ImmutableArray<(string name, string? alias)> s_usingNamespaceDirectives = ImmutableArray.Create<(string name, string? alias)>();

        public static IBindingManager? BindingManager
        {
            get
            {
                EnsureInitialized();
                return s_bindingManager;
            }
        }
        public static ITargetProvider<ITarget>? TargetProvider
        {
            get
            {
                EnsureInitialized();
                return s_targetProvider;
            }
        }
        public static ILoaderProvider<ITarget>? LoaderProvider
        {
            get
            {
                EnsureInitialized();
                return s_loaderProvider;
            }
        }
        public static IHookInitializerProvider? HookInitializerProvider
        {
            get
            {
                EnsureInitialized();
                return s_hookInitializerProvider;
            }
        }
        public static ImmutableArray<Reference> References
        {
            get
            {
                EnsureInitialized();
                return s_references;
            }
        }
        public static ImmutableArray<Type> UsingStaticTypes
        {
            get
            {
                EnsureInitialized();
                return s_usingStaticTypes;
            }
        }
        public static ImmutableArray<(string name, string? alias)> UsingNamespaceDirectives
        {
            get
            {
                EnsureInitialized();
                return s_usingNamespaceDirectives;
            }
        }

    }

}
