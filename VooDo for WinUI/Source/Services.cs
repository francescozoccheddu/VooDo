namespace VooDo.WinUI
{

    public static class Services
    {

        public static ServiceProviderManager<ILoaderProvider> LoaderProvider { get; }
            = ServiceProviderManager<ILoaderProvider>.CreateFromAttribute<LoaderProviderAttribute>();

    }

}
