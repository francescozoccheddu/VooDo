﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record BlockStatement : SingleStatement, IReadOnlyList<Statement>
    {

        #region Members

        private ImmutableArray<Statement> m_statements;
        private ImmutableArray<Statement> m_Statements
        {
            get => m_statements;
            init => m_statements = value.EmptyIfDefault();
        }

        public BlockStatement(ImmutableArray<Statement> _statements) => m_Statements = _statements;

        #endregion

        #region Overrides

        public override BlockStatement ReplaceNodes(Func<Node?, Node?> _map)
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
        internal override BlockSyntax EmitNode(Scope _scope, Tagger _tagger)
        {
            Scope nestedScope = _scope.CreateNested();
            IEnumerable<StatementSyntax> statements = this.SelectMany(_s => _s.EmitNodes(nestedScope, _tagger));
            return SyntaxFactory.Block(statements.ToSyntaxList()).Own(_tagger, this);
        }

        public override IEnumerable<Statement> Children => m_Statements;
        public override string ToString() => Count == 0
            ? "{}"
            : $"{{{("\n" + string.Join('\n', m_Statements)).Replace("\n", "\n\t")}\n}}";

        #endregion

    }

}