
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;
using VooDo.AST.Statements;
using VooDo.Utils;

namespace VooDo.AST
{

    public sealed record Script(ImmutableArray<UsingDirective> Usings, ImmutableArray<Statement> Statements) : Node
    {

        private ImmutableArray<UsingDirective> m_usings = Usings.EmptyIfDefault();
        public ImmutableArray<UsingDirective> Usings
        {
            get => m_usings;
            init => m_usings = value.EmptyIfDefault();
        }

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<UsingDirective> newUsings = Usings.Map(_map).NonNull();
            ImmutableArray<Statement> newStatements = Statements.Map(_map).NonNull();
            if (newUsings == Usings && newStatements == Statements)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Usings = newUsings,
                    Statements = newStatements
                };
            }
        }

        public override IEnumerable<Node> Children => ((IEnumerable<Node>) Usings).Concat(Statements);
        public override string ToString() => (string.Join("\n", Usings) + "\n\n" + string.Join("\n", Statements)).Trim();

    }

}
