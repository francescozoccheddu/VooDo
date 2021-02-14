using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record GlobalStatement : Statement, IReadOnlyList<DeclarationStatement>
    {

        #region Members

        private ImmutableArray<DeclarationStatement> m_declarations;
        private ImmutableArray<DeclarationStatement> m_Declarations
        {
            get => m_declarations;
            init => m_declarations = value.EmptyIfDefault();
        }

        public GlobalStatement(ImmutableArray<DeclarationStatement> _declarations) => m_Declarations = _declarations;

        #endregion

        #region Overrides

        public override GlobalStatement ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ImmutableArray<DeclarationStatement> newDeclarations = m_Declarations.Map(_map).NonNull();
            if (newDeclarations == m_Declarations)
            {
                return this;
            }
            else
            {
                return this with
                {
                    m_Declarations = newDeclarations
                };
            }
        }

        public DeclarationStatement this[int _index] => ((IReadOnlyList<DeclarationStatement>) m_Declarations)[_index];
        public int Count => ((IReadOnlyCollection<Statement>) m_Declarations).Count;
        public IEnumerator<DeclarationStatement> GetEnumerator() => ((IEnumerable<DeclarationStatement>) m_Declarations).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_Declarations).GetEnumerator();
        internal override BlockSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.Block(this.Select(_s => _s.EmitNode(_scope, _tagger, true)).ToSyntaxList()).Own(_tagger, this);
        public override IEnumerable<DeclarationStatement> Children => m_Declarations;
        public override string ToString() => GrammarConstants.globalKeyword + Count switch
        {
            0 => " {}",
            1 => $" {this[0]}",
            _ => $"{{{("\n" + string.Join('\n', this)).Replace("\n", "\n\t")}\n}}"
        };

        #endregion

    }

}
