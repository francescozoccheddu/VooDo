using VooDo.AST.Expressions;
using VooDo.AST.Names;

namespace VooDo.Compilation
{

    public sealed record Global(ComplexTypeOrVar Type, Identifier? Name, Expression? Initializer = null)
    {

        public bool IsAnonymous => Name is null;
        public bool HasInitializer => Initializer is not null;

    }


}
