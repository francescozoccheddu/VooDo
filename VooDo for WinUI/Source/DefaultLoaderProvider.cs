using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Hooks;

namespace VooDo.WinUI
{

    public sealed class DefaultLoaderProvider : LoaderProvider
    {

        public sealed class HookInitializerProviderAttribute : CombinableServiceProviderAttribute
        {

            internal static (IHookInitializerProvider provider, int priority)? GetHookInitializerProvider()
                => GetProvider<IHookInitializerProvider, HookInitializerProviderAttribute>(_h => new HookInitializerList(_h));

        }

        public static ServiceProviderManager<IHookInitializerProvider> HookInitializerProviderServiceManager { get; }
            = new(new HookInitializerList(Enumerable.Empty<IHookInitializerProvider>()));

        public static List<Reference> AdditionalReferences { get; } = new();

        private static DefaultLoaderProvider? s_instance;

        internal static DefaultLoaderProvider GetInstance()
        {
            if (s_instance is null)
            {
                s_instance = new();
                (IHookInitializerProvider provider, int priority)? result = HookInitializerProviderAttribute.GetHookInitializerProvider();
                if (result is not null)
                {
                    HookInitializerProviderServiceManager.RegisterProvider(result.Value.provider, result.Value.priority);
                }
            }
            return s_instance;
        }


        private DefaultLoaderProvider()
        {

        }

        protected override IHookInitializerProvider GetHookInitializerProvider(Target _target)
            => HookInitializerProviderServiceManager.Provider!;

        protected override ImmutableArray<Reference> GetReferences(Target _target)
            => Reference.GetSystemReferences().Add(Reference.RuntimeReference).AddRange(AdditionalReferences);

        protected override ILoaderCache? m_LoaderCache { get; } = new LoaderMemoryCache(32);

    }

}
