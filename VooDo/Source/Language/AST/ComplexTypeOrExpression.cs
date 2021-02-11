using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Compilation;
using VooDo.Language.Linking;

namespace VooDo.Language.AST
{

    public abstract record ComplexTypeOrExpression : Node
    {

        internal abstract override ExpressionSyntax EmitNode(Scope _scope, Marker _marker);

    }

}
