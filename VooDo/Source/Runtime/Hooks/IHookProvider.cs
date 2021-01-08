using System;

using VooDo.AST;

namespace VooDo.Runtime.Hooks
{
    public interface IHookProvider
    {

        Type Type { get; }

        IHook Subscribe(Eval _instance, Name _property);

    }
}
