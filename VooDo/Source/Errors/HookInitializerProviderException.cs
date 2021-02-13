using System;

using VooDo.Hooks;

namespace VooDo.Errors
{

    public sealed class HookInitializerProviderException : UserCodeException
    {

        public IHookInitializerProvider HookInitializerProvider { get; }

        internal HookInitializerProviderException(IHookInitializerProvider _hookInitializerProvider, Exception _exception)
            : base("Exception while running HookInitializerProvider code", _exception)
        {
            HookInitializerProvider = _hookInitializerProvider;
        }

    }

}
