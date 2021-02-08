using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;
using VooDo.Utils;

namespace VooDo.Language.AST.Statements
{

    public record DeclarationStatement(ComplexTypeOrVar Type, ImmutableArray<DeclarationStatement.Declarator> Declarators) : Statement
    {

        #region Nested types

        public sealed record Declarator(Identifier Name, Expression? Initializer = null) : Node
        {

            public bool HasInitializer => Initializer is not null;

            public override IEnumerable<Node> Children => HasInitializer ? new Node[] { Initializer! } : Enumerable.Empty<Node>();
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

        public override IEnumerable<Node> Children => new Node[] { Type }.Concat(Declarators);
        public override string ToString() => $"{Type} {string.Join(", ", Declarators)}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
