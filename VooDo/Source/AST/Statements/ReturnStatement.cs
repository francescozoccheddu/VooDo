using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compilation;

namespace VooDo.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : Statement
    {

        #region Overrides

        public override ArrayCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            ImmutableArray<Expression> newSizes = Sizes.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newSizes == Sizes)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Sizes = newSizes
                };
            }
        }

        internal override StatementSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ReturnStatement(Expression.EmitNode(_scope, _marker)).Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
