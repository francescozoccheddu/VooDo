using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST
{

    public abstract record ComplexTypeOrExpression : BodyNode
    {

        internal abstract override ExpressionSyntax EmitNode(Scope _scope, Marker _marker);

    }

}
