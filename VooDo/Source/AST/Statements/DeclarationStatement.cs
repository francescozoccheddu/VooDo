
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public record DeclarationStatement : Statement
    {

        
        public sealed record Declarator(Identifier Name, Expression? Initializer = null) : BodyNode
        {

            public bool HasInitializer => Initializer is not null;

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

            public override IEnumerable<Node> Children
                => HasInitializer ? new Node[] { Name, Initializer! } : new Node[] { Name };
            public override string ToString() => HasInitializer ? $"{Name} {AssignmentStatement.EKind.Simple.Token()} {Initializer}" : $"{Name}";

        }

        
        
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

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        public override IEnumerable<Node> Children => new BodyNode[] { Type }.Concat(Declarators);
        public override string ToString() => $"{Type} {string.Join(", ", Declarators)}{GrammarConstants.statementEndToken}";

        
    }

}
