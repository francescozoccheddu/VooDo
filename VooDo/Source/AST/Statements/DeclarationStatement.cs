using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public record DeclarationStatement : Statement
    {

        #region Nested types

        public sealed record Declarator(Identifier Name, Expression? Initializer = null) : BodyNode
        {

            public bool HasInitializer => Initializer is not null;

            internal override VariableDeclaratorSyntax EmitNode(Scope _scope, Tagger _tagger)
            {
                ExpressionSyntax? initializer;
                if (Parent is not null)
                {
                    Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new GlobalPrototype(new Global(Parent.Type, Name, Initializer), this));
                    initializer = SyntaxFactoryHelper.ThisMemberAccess(globalDefinition.Identifier);
                }
                else
                {
                    _scope.AddLocal(Name);
                    initializer = Initializer?.EmitNode(_scope, _tagger);
                }
                EqualsValueClauseSyntax? initializerClause = initializer?.ToEqualsValueClause();
                return SyntaxFactory.VariableDeclarator(Name.EmitToken(_tagger), null, initializerClause).Own(_tagger, this);
            }

            public override Declarator ReplaceNodes(Func<Node?, Node?> _map)
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

            public override DeclarationStatement? Parent => (DeclarationStatement?) base.Parent;

            public override IEnumerable<Node> Children
                => HasInitializer ? new Node[] { Name, Initializer! } : new Node[] { Name };
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

        public override DeclarationStatement ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override LocalDeclarationStatementSyntax EmitNode(Scope _scope, Tagger _tagger)
        {
            TypeSyntax type = Type.EmitNode(_scope, _tagger);
            if (Parent is GlobalStatement && !Type.IsVar)
            {
                type = SyntaxFactoryHelper.VariableType(type);
            }
            return SyntaxFactory.LocalDeclarationStatement(
                           SyntaxFactory.VariableDeclaration(type,
                               Declarators.Select(_d => _d.EmitNode(_scope, _tagger)).ToSeparatedList()))
                       .Own(_tagger, this);
        }

        public override IEnumerable<BodyNode> Children => new BodyNode[] { Type }.Concat(Declarators);
        public override string ToString() => $"{Type} {string.Join(", ", Declarators)}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
