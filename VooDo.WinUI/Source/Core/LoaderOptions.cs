using System;
using System.Collections.Immutable;

using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.WinUI.Attributes;
using VooDo.WinUI.Components;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Core
{

    public static class LoaderOptions
    {

        public static ImmutableArray<Reference> References { get; private set; }
        public static IHookInitializer HookInitializer { get; private set; } = null!;
        public static ITargetProvider<ITarget> TargetProvider { get; private set; } = null!;
        public static ImmutableArray<(string name, string? alias)> UsingNamespaceDirectives { get; private set; }
        public static ImmutableArray<Type> UsingStaticTypes { get; private set; }

        static LoaderOptions()
        {
            Update();
        }

        internal static void Update()
        {
            References = Defaults.References.AddRange(AttributeManager.References);
            HookInitializer = AttributeManager.HookInitializer is null
                ? Defaults.HookInitializer
                : new HookInitializerList(new[] { Defaults.HookInitializer, AttributeManager.HookInitializer });
            TargetProvider = AttributeManager.TargetProvider is null
                ? Defaults.TargetProvider
                : new TargetProviderList<ITarget>(new[] { Defaults.TargetProvider, AttributeManager.TargetProvider });
            UsingNamespaceDirectives = Defaults.UsingNamespaceDirectives.AddRange(AttributeManager.UsingNamespaceDirectives);
            UsingStaticTypes = Defaults.UsingStaticTypes.AddRange(AttributeManager.UsingStaticTypes);
        }

    }

}
