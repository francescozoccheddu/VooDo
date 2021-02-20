
using VooDo.AST.Names;

namespace VooDo.WinUI.Options
{

    public sealed record UsingNamespace
    {

        public UsingNamespace(Namespace _namespace, Identifier? _alias = null)
        {
            Namespace = _namespace;
            Alias = _alias;
        }

        public Namespace Namespace { get; init; }
        public Identifier? Alias { get; init; }

    }

}
