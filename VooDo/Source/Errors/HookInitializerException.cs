using System;

using VooDo.Hooks;

namespace VooDo.Errors
{

    public sealed class HookInitializerException : UserCodeException
    {

        public IHookInitializer HookInitializer { get; }

        internal HookInitializerException(IHookInitializer _hookInitializer, Exception _exception)
            : base("Exception while running HookInitializer code", _exception)
        {
            HookInitializer = _hookInitializer;
        }


    }

}
