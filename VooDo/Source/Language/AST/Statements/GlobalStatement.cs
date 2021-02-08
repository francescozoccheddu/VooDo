using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Utils;

namespace VooDo.Language.AST.Statements
{

    public sealed record GlobalStatement : Statement, IReadOnlyList<DeclarationStatement>
    {

        #region Members

        private readonly ImmutableArray<DeclarationStatement> m_declarations;

        public GlobalStatement(ImmutableArray<DeclarationStatement> _declarations) => m_declarations = _declarations.EmptyIfDefault();

        #endregion

        #region Overrides

        public DeclarationStatement this[int _index] => ((IReadOnlyList<DeclarationStatement>) m_declarations)[_index];
        public int Count => ((IReadOnlyCollection<Statement>) m_declarations).Count;
        public IEnumerator<DeclarationStatement> GetEnumerator() => ((IEnumerable<DeclarationStatement>) m_declarations).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_declarations).GetEnumerator();
        public override string ToString() => GrammarConstants.globalKeyword + Count switch
        {
            0 => " {}",
            1 => $" {this[0]}",
            _ => $"{{{("\n" + string.Join('\n', this)).Replace("\n", "\n\t")}\n}}"
        };

        #endregion

    }

}
