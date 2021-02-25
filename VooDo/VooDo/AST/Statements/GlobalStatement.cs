
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record GlobalStatement : Statement, IReadOnlyList<DeclarationStatement>
    {

        
        private ImmutableArray<DeclarationStatement> m_declarations;
        private ImmutableArray<DeclarationStatement> m_Declarations
        {
            get => m_declarations;
            init => m_declarations = value.EmptyIfDefault();
        }

        public bool IsConstant { get; }

        public GlobalStatement(bool _isConstant, ImmutableArray<DeclarationStatement> _declarations)
        {
            IsConstant = _isConstant;
            m_Declarations = _declarations;
        }

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        public override IEnumerable<Node> Children => m_Declarations;
        public override string ToString() => (IsConstant ? GrammarConstants.constKeyword : GrammarConstants.globalKeyword) + Count switch
        {
            0 => " {}",
            1 => $" {this[0]}",
            _ => $"{{{("\n" + string.Join("\n", this)).Replace("\n", "\n\t")}\n}}"
        };

        
    }

}
