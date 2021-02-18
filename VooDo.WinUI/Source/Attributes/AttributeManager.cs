using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.Hooks;
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
            s_initialized = true;
            ImmutableArray<Assembly> assemblies = GetAssemblies();
            if (s_lastAssemblies.SequenceEqual(assemblies))
            {
                return;
            }
            s_lastAssemblies = assemblies;
            {
                // Assembly attributes
                ImmutableDictionary<Type, ImmutableArray<AssemblyTagAttribute>> assemblyMap = AssemblyTagAttribute.RetrieveAttributes(assemblies);
                References = ReferenceAttribute.Resolve(assemblyMap[typeof(ReferenceAttribute)].Cast<ReferenceAttribute>());
            }
            {
                // Type attributes
                ImmutableDictionary<Type, ImmutableArray<TypeTagAttribute.Application>> typeMap = TypeTagAttribute.RetrieveAttributes(assemblies);
                BindingManager = BindingManagerAttribute.Resolve(typeMap[typeof(BindingManagerAttribute)]);
                TargetProvider = TargetProviderAttribute.Resolve<ITarget>(typeMap[typeof(TargetProviderAttribute)]);
                LoaderProvider = LoaderProviderAttribute.Resolve<ITarget>(typeMap[typeof(LoaderProviderAttribute)]);
                HookInitializerProvider = HookInitializerProviderAttribute.Resolve(typeMap[typeof(HookInitializerProviderAttribute)]);
            }
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

        public static IBindingManager? BindingManager
        {
            get
            {
                EnsureInitialized();
                return s_bindingManager;
            }
            private set => s_bindingManager = value;
        }
        public static ITargetProvider<ITarget>? TargetProvider
        {
            get
            {
                EnsureInitialized();
                return s_targetProvider;
            }
            private set => s_targetProvider = value;
        }
        public static ILoaderProvider<ITarget>? LoaderProvider
        {
            get
            {
                EnsureInitialized();
                return s_loaderProvider;
            }
            private set => s_loaderProvider = value;
        }
        public static IHookInitializerProvider? HookInitializerProvider
        {
            get
            {
                EnsureInitialized();
                return s_hookInitializerProvider;
            }
            private set => s_hookInitializerProvider = value;
        }
        public static ImmutableArray<Reference> References
        {
            get
            {
                EnsureInitialized();
                return s_references;
            }
            private set => s_references = value;
        }

    }

}
