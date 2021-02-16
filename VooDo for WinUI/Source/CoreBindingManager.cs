using VooDo.WinUI.Components;
using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI
{

    internal static class CoreBindingManager
    {

        internal static BindingManager BindingManager { get; }

        static CoreBindingManager()
        {
            (ITargetProvider targetProvider, _) = CombinableServiceProviderAttribute.GetProvider<ITargetProvider, TargetProviderAttribute>(_p => new TargetProviderList(_p));
            (ILoaderProvider provider, int)? loaderProvider = ServiceProviderAttribute.GetProvider<ILoaderProvider, LoaderProviderAttribute>();
            BindingManager = new BindingManager(targetProvider, loaderProvider?.provider ?? DefaultLoaderProvider.GetInstance());
        }

    }

}
