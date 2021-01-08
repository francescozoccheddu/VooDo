using System;

using VooDo.AST;

namespace VooDo.Runtime.Hooks
{
    public abstract class TypedHookProvider<T> : IHookProvider
    {

        protected abstract IHook Subscribe(T _instance, Name _property);

        Type IHookProvider.Type => typeof(T);

        IHook IHookProvider.Subscribe(Eval _instance, Name _property)
            => _instance.Value is T t ? Subscribe(t, _property) : null;

    }
}
