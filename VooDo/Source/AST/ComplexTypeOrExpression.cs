using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Compilation.Emission;

namespace VooDo.AST
{

    public abstract record ComplexTypeOrExpression : Node
    {

        public abstract override ComplexTypeOrExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

        internal abstract override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger);

    }

}
