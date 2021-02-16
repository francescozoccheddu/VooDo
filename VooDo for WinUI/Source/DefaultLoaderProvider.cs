using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Hooks;

namespace VooDo.WinUI
{

    internal sealed class DefaultLoaderProvider : LoaderProvider
    {

        internal DefaultLoaderProvider()
        {
            m_References = AppDomain.CurrentDomain.GetAssemblies().Select(_a => Reference.FromAssembly(_a)).ToImmutableArray();
            m_ProgramCache = new ProgramCache();
            m_HookInitializerProvider = HookInitializerProviderAttribute.GetProvider() ?? new HookInitializerList(Enumerable.Empty<IHookInitializerProvider>());
        }

        protected override IHookInitializerProvider m_HookInitializerProvider { get; }
        protected override IEnumerable<Reference> m_References { get; }
        protected override IProgramCache m_ProgramCache { get; }

    }

}
