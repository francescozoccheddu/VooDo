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

    public sealed record GlobalStatement : Statement, IReadOnlyList<DeclarationStatement>
    {

        #region Members

        private readonly ImmutableArray<DeclarationStatement> m_declarations;

        public GlobalStatement(ImmutableArray<DeclarationStatement> _declarations) => m_declarations = _declarations.EmptyIfDefault();

        #endregion

        #region Overrides

        public override ArrayCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            ImmutableArray<Expression> newSizes = Sizes.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newSizes == Sizes)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Sizes = newSizes
                };
            }
        }

        public DeclarationStatement this[int _index] => ((IReadOnlyList<DeclarationStatement>) m_declarations)[_index];
        public int Count => ((IReadOnlyCollection<Statement>) m_declarations).Count;
        public IEnumerator<DeclarationStatement> GetEnumerator() => ((IEnumerable<DeclarationStatement>) m_declarations).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_declarations).GetEnumerator();
        internal override BlockSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.Block(this.Select(_s => _s.EmitNode(_scope, _marker, true)).ToSyntaxList()).Own(_marker, this);
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
