
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record BlockStatement : Statement, IReadOnlyList<Statement>
    {

        
        private ImmutableArray<Statement> m_statements;
        private ImmutableArray<Statement> m_Statements
        {
            get => m_statements;
            init => m_statements = value.EmptyIfDefault();
        }

        public BlockStatement(ImmutableArray<Statement> _statements) => m_Statements = _statements;

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<Statement> newStatements = m_statements.Map(_map).NonNull();
            if (newStatements == m_statements)
            {
                return this;
            }
            else
            {
                return this with
                {
                    m_Statements = newStatements
                };
            }
        }

        public Statement this[int _index] => ((IReadOnlyList<Statement>) m_Statements)[_index];
        public int Count => ((IReadOnlyCollection<Statement>) m_Statements).Count;
        public IEnumerator<Statement> GetEnumerator() => ((IEnumerable<Statement>) m_Statements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_Statements).GetEnumerator();


        public override IEnumerable<Node> Children => m_Statements;
        public override string ToString() => Count == 0
            ? "{}"
            : $"{{{("\n" + string.Join("\n", m_Statements)).Replace("\n", "\n\t")}\n}}";

        
    }

}
