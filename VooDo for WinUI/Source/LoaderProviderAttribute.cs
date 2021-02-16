
using VooDo.WinUI;

namespace VooDo.WinUi
{

    public sealed class LoaderProviderAttribute : ServiceProviderAttribute
    {

        internal static ILoaderProvider? GetProvider() => GetProvider<ILoaderProvider?, LoaderProviderAttribute>();

        public LoaderProviderAttribute()
        { }

    }

}
