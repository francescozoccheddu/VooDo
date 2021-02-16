namespace VooDo.WinUI
{

    public delegate void ServiceProviderChangedEventHandler<TService>(ServiceProviderManager<TService> _manager, TService? _old) where TService : notnull;

    public sealed class ServiceProviderManager<TService> where TService : notnull
    {

        public int Priority { get; private set; } = int.MinValue;
        public TService? Provider { get; private set; }
        public event ServiceProviderChangedEventHandler<TService>? OnProviderChanged;

        internal static ServiceProviderManager<TService> CreateFromAttribute<TAttribute>() where TAttribute : ServiceProviderAttribute
        {
            ServiceProviderManager<TService> manager = Create();
            (TService provider, int priority)? result = ServiceProviderAttribute.GetProvider<TService, TAttribute>();
            if (result is not null)
            {
                manager.RegisterProvider(result.Value.provider, result.Value.priority);
            }
            return manager;
        }

        internal static ServiceProviderManager<TService> Create()
            => new ServiceProviderManager<TService>();

        private ServiceProviderManager() { }

        public void RegisterProvider(TService _provider)
            => RegisterProvider(_provider, Priority);

        public void RegisterProvider(TService _provider, int _priority)
        {
            if (_priority >= Priority)
            {
                TService? old = Provider;
                Priority = _priority;
                Provider = _provider;
                if (!ReferenceEquals(old, _provider))
                {
                    OnProviderChanged?.Invoke(this, old);
                }
            }
        }

    }

}
