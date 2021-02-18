using System;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;

namespace VooDo.WinUI.Interfaces
{

    public interface ILoaderProvider<in TTarget> where TTarget : ITarget
    {

        Loader GetLoader(Script _source, TTarget _target);
        ComplexType GetTypeNode(Type _type, TTarget _target);

    }

}
