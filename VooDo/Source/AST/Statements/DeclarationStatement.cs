using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public record DeclarationStatement : Statement
    {

        #region Nested types

        public sealed record Declarator(Identifier Name, Expression? Initializer = null) : Node
        {

            public bool HasInitializer => Initializer is not null;

            internal VariableDeclaratorSyntax EmitNode(Scope _scope, Marker _marker, DeclarationStatement? _declarationStatement)
            {
                ExpressionSyntax? initializer;
                if (_declarationStatement is not null)
                {
                    Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new GlobalPrototype(new Global(_declarationStatement.Type, Name, Initializer), _declarationStatement, this));
                    initializer = SyntaxFactoryHelper.ThisMemberAccess(globalDefinition.Identifier);
                }
                else
                {
                    _scope.AddLocal(Name);
                    initializer = Initializer?.EmitNode(_scope, _marker);
                }
                EqualsValueClauseSyntax? initializerClause = initializer?.ToEqualsValueClause();
                return SyntaxFactory.VariableDeclarator(Name.EmitToken(_marker), null, initializerClause).Own(_marker, this);
            }

            public override Declarator ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
            {
                Identifier newName = (Identifier) _map(Name).NonNull();
                Expression? newInitializer = (Expression?) _map(Initializer);
                if (ReferenceEquals(newName, Name) && ReferenceEquals(newInitializer, Initializer))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Name = newName,
                        Initializer = newInitializer
                    };
                }
            }

            internal override VariableDeclaratorSyntax EmitNode(Scope _scope, Marker _marker) => EmitNode(_scope, _marker, null);
            public override IEnumerable<NodeOrIdentifier> Children
                => HasInitializer ? new NodeOrIdentifier[] { Name, Initializer! } : new NodeOrIdentifier[] { Name };
            public override string ToString() => HasInitializer ? $"{Name} {AssignmentStatement.EKind.Simple.Token()} {Initializer}" : $"{Name}";

        }

        #endregion

        #region Members

        public DeclarationStatement(ComplexTypeOrVar _type, ImmutableArray<Declarator> _declarators)
        {
            Type = _type;
            Declarators = _declarators;
        }

        public ComplexTypeOrVar Type { get; init; }

        private ImmutableArray<Declarator> m_declarators;
        public ImmutableArray<Declarator> Declarators
        {
            get => m_declarators;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "DeclarationStatement must have at least one declarator").AsThrowable();
                }
                m_declarators = value;
            }

        }

        #endregion

        #region Overrides

        protected override IEnumerable<Problem> GetSelfSyntaxProblems()
        {
            if (Type.IsVar)
            {
                return Declarators
                    .Where(_d => !_d.HasInitializer)
                    .Select(_d => new ChildSyntaxError(this, _d, "A variable declaration with var type must provide an initializer"))
                    .Concat(
                        Declarators
                        .Where(_d => _d.Initializer is DefaultExpression expression && !expression.HasType)
                        .Select(_d => new ChildSyntaxError(this, _d, "A variable declaration with var type cannot have a non-typed default initializer")));
            }
            else
            {
                return Enumerable.Empty<Problem>();
            }
        }

        public override DeclarationStatement ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexTypeOrVar newType = (ComplexTypeOrVar) _map(Type).NonNull();
            ImmutableArray<Declarator> newDeclarators = Declarators.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newDeclarators == Declarators)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Declarators = newDeclarators
                };
            }
        }

        internal LocalDeclarationStatementSyntax EmitNode(Scope _scope, Marker _marker, bool _global)
        {
            TypeSyntax type = Type.EmitNode(_scope, _marker);
            if (_global && !Type.IsVar)
            {
                type = SyntaxFactoryHelper.VariableType(type);
            }
            return SyntaxFactory.LocalDeclarationStatement(
                           SyntaxFactory.VariableDeclaration(type,
                               Declarators.Select(_d => _d.EmitNode(_scope, _marker, _global ? this : null)).ToSeparatedList()))
                       .Own(_marker, this);
        }

        internal override LocalDeclarationStatementSyntax EmitNode(Scope _scope, Marker _marker) => EmitNode(_scope, _marker, false);
        public override IEnumerable<Node> Children => new Node[] { Type }.Concat(Declarators);
        public override string ToString() => $"{Type} {string.Join(", ", Declarators)}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
