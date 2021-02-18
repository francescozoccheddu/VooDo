
using System.IO;

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
                    LoaderDiskCache cache = new LoaderDiskCache(Path.Combine(Path.GetTempPath(), "ciao"));
                    SimpleLoaderProvider? loaderProvider = new SimpleLoaderProvider(Defaults.References, Defaults.HookInitializerProvider, cache);
                    s_bindingManager = new BindingManager<SimpleTarget>(Defaults.TargetProvider, loaderProvider);
                }
                return s_bindingManager;
            }
            set => s_bindingManager = value;
        }

    }

}
