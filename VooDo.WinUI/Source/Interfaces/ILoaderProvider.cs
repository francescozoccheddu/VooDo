using System;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;

namespace VooDo.WinUI.Interfaces
{

    public interface ILoaderProvider
    {

        Loader GetLoader(Script _source, ITarget _target);
        ComplexType GetTypeNode(Type _type, ITarget _target);

    }

    public interface ILoaderProvider<in TTarget> : ILoaderProvider where TTarget : ITarget
    {

        Loader ILoaderProvider.GetLoader(Script _source, ITarget _target)
            => GetLoader(_source, (TTarget) _target);

        ComplexType ILoaderProvider.GetTypeNode(Type _type, ITarget _target)
            => GetTypeNode(_type, (TTarget) _target);

        Loader GetLoader(Script _source, TTarget _target);
        ComplexType GetTypeNode(Type _type, TTarget _target);

    }

}
