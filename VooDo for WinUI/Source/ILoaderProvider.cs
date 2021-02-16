using System;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;

namespace VooDo.WinUI
{

    public interface ILoaderProvider
    {

        Loader GetLoader(Script _script, Target _target);
        ComplexType GetTypeNode(Type _type, Target _target);

    }

    public sealed class LoaderProviderAttribute : ServiceProviderAttribute { }

}
