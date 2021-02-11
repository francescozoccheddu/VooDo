using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Statements
{

    public record DeclarationStatement(ComplexTypeOrVar Type, ImmutableArray<DeclarationStatement.Declarator> Declarators) : Statement
    {

        #region Nested types

        public sealed record Declarator(Identifier Name, Expression? Initializer = null) : Node
        {

            public bool HasInitializer => Initializer is not null;

            internal VariableDeclaratorSyntax EmitNode(Scope _scope, Marker _marker, ComplexTypeOrVar? _globalType)
            {
                ExpressionSyntax? initializer;
                if (_globalType is not null)
                {
                    Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new Global(_globalType, Name, Initializer));
                    initializer = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(globalDefinition.Identifier));
                }
                else
                {
                    _scope.AddLocal(Name);
                    initializer = Initializer?.EmitNode(_scope, _marker);
                }
                EqualsValueClauseSyntax? initializerClause = initializer is null ? null : SyntaxFactory.EqualsValueClause(initializer);
                return SyntaxFactory.VariableDeclarator(Name.EmitToken(_marker), null, initializerClause).Own(_marker, this);
            }
            internal override VariableDeclaratorSyntax EmitNode(Scope _scope, Marker _marker) => EmitNode(_scope, _marker, null);
            public override IEnumerable<NodeOrIdentifier> Children
                => (HasInitializer ? new NodeOrIdentifier[] { Initializer! } : Enumerable.Empty<NodeOrIdentifier>()).Append(Name);
            public override string ToString() => HasInitializer ? $"{Name}" : $"{Name} {AssignmentStatement.EKind.Simple.Token()} {Initializer}";

        }

        #endregion

        #region Members

        private ImmutableArray<Declarator> m_declarators = Declarators.NonEmpty();
        public ImmutableArray<Declarator> Declarators
        {
            get => m_declarators;
            init => m_declarators = value.NonEmpty();

        }

        #endregion

        #region Overrides

        internal LocalDeclarationStatementSyntax EmitNode(Scope _scope, Marker _marker, bool _global)
        {
            TypeSyntax type = Type.EmitNode(_scope, _marker);
            if (_global && !Type.IsVar)
            {
                type = SyntaxFactoryHelper.VariableType(type);
            }
            return SyntaxFactory.LocalDeclarationStatement(
                           SyntaxFactory.VariableDeclaration(type,
                               Declarators.Select(_d => _d.EmitNode(_scope, _marker, _global ? Type : null)).ToSeparatedList()))
                       .Own(_marker, this);
        }

        internal override LocalDeclarationStatementSyntax EmitNode(Scope _scope, Marker _marker) => EmitNode(_scope, _marker, false);
        public override IEnumerable<Node> Children => new Node[] { Type }.Concat(Declarators);
        public override string ToString() => $"{Type} {string.Join(", ", Declarators)}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
