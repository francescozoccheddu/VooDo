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

        public static ImmutableArray<Reference> References { get; }
        public static IHookInitializerProvider HookInitializerProvider { get; }
        public static ITargetProvider<SimpleTarget> TargetProvider { get; }

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
                References = references.ToImmutableArray();
            }
            HookInitializerProvider = new HookInitializerList(Enumerable.Empty<IHookInitializerProvider>());
            TargetProvider = new SimpleTargetProvider();
        }

    }

}
