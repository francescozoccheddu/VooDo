using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Utils;

namespace VooDo.Language.AST.Statements
{

    public sealed record BlockStatement : Statement, IReadOnlyList<Statement>
    {

        #region Members

        private readonly ImmutableArray<Statement> m_statements;

        public BlockStatement(ImmutableArray<Statement> _statements) => m_statements = _statements.EmptyIfDefault();

        #endregion

        #region Overrides

        public Statement this[int _index] => ((IReadOnlyList<Statement>) m_statements)[_index];
        public int Count => ((IReadOnlyCollection<Statement>) m_statements).Count;
        public IEnumerator<Statement> GetEnumerator() => ((IEnumerable<Statement>) m_statements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_statements).GetEnumerator();
        public override string ToString() => Count == 0
            ? "{}"
            : $"{{{("\n" + string.Join('\n', this)).Replace("\n", "\n\t")}\n}}";

        #endregion

    }

}
