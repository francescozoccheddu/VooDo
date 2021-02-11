using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;

namespace VooDo.Language.Linking
{

    public sealed record Global(ComplexTypeOrVar Type, Identifier? Name, Expression? Initializer = null)
    {

        public bool IsAnonymous => Name is null;
        public bool HasInitializer => Initializer is not null;

    }


}
