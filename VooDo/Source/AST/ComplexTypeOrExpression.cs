using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Compiling.Emission;

namespace VooDo.AST
{

    public abstract record ComplexTypeOrExpression : BodyNode
    {

        public abstract override ComplexTypeOrExpression ReplaceNodes(Func<Node?, Node?> _map);

        internal abstract override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger);

    }

}
