using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.Linking;
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
        internal override BlockSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.Block(SyntaxFactory.List(this.Select(_s => _s.EmitNode(_scope, _marker, true)))).Own(_marker, this);
        public override IEnumerable<DeclarationStatement> Children => m_declarations;
        public override string ToString() => GrammarConstants.globalKeyword + Count switch
        {
            0 => " {}",
            1 => $" {this[0]}",
            _ => $"{{{("\n" + string.Join('\n', this)).Replace("\n", "\n\t")}\n}}"
        };

        #endregion

    }

}
