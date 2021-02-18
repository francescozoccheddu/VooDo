
using VooDo.Caching;
using VooDo.WinUI.Components;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Core
{

    public static class CoreBindingManager
    {

        private static IBindingManager? s_bindingManager;

        public static IBindingManager BindingManager
        {
            get
            {
                if (s_bindingManager is null)
                {
                    SimpleLoaderProvider? loaderProvider = new SimpleLoaderProvider(Defaults.References, Defaults.HookInitializerProvider, new LoaderMemoryCache());
                    s_bindingManager = new BindingManager<SimpleTarget>(Defaults.TargetProvider, loaderProvider);
                }
                return s_bindingManager;
            }
            set => s_bindingManager = value;
        }

    }

}
