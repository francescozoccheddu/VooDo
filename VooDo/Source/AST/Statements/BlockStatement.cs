using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Statements
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
        internal override BlockSyntax EmitNode(Scope _scope, Marker _marker)
        {
            Scope nestedScope = _scope.CreateNested();
            IEnumerable<StatementSyntax> statements = this.SelectMany(_s => _s is GlobalStatement globals
                        ? globals.EmitNode(nestedScope, _marker).Statements
                        : _s.EmitNode(nestedScope, _marker).ToSyntaxList());
            return SyntaxFactory.Block(statements.ToSyntaxList()).Own(_marker, this);
        }

        public override IEnumerable<Statement> Children => m_statements;
        public override string ToString() => Count == 0
            ? "{}"
            : $"{{{("\n" + string.Join('\n', m_statements)).Replace("\n", "\n\t")}\n}}";

        #endregion

    }

}
