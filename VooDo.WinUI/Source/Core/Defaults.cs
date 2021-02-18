using Microsoft.UI.Xaml;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.WinUI.Components;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Core
{

    public static class Defaults
    {

        private static readonly ImmutableArray<Reference> s_references;
        private static readonly IHookInitializerProvider s_hookInitializerProvider;
        private static readonly ILoaderProvider<ITarget> s_loaderProvider;
        private static readonly ITargetProvider<ITarget> s_targetProvider;
        private static readonly IBindingManager s_bindingManager;

        static Defaults()
        {
            {
                List<Reference> references = new();
                references.Add(Reference.RuntimeReference);
                references.Add(Reference.FromAssembly(typeof(DependencyObject).Assembly));
                references.AddRange(Reference.GetSystemReferences());
                AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
                references.AddRange(AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(_a => _a.GetReferencedAssemblies().Contains(assemblyName))
                    .Select(_a => Reference.FromAssembly(_a)));
                s_references = references.ToImmutableArray();
            }
            s_hookInitializerProvider = new HookInitializerList(Enumerable.Empty<IHookInitializerProvider>());
            s_loaderProvider = new SimpleLoaderProvider();
            s_targetProvider = new SimpleTargetProvider();
            s_bindingManager = new BindingManager<ITarget>(s_targetProvider, s_loaderProvider);
        }

        internal static void Update()
        {

        }

    }

}
