using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ArrayCreationExpression(ComplexType Type, ImmutableArray<Expression> Sizes) : Expression
    {

        #region Delegating constructors

        public ArrayCreationExpression(params Expression[] _sizes) : this(_sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ComplexType? _type, params Expression[] _sizes) : this(_type, _sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(IEnumerable<Expression> _sizes) : this(_sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ComplexType? _type, IEnumerable<Expression> _sizes) : this(_type, _sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ImmutableArray<Expression> _sizes) : this(null, _sizes) { }

        #endregion

        #region Members

        private ComplexType m_type = Type.Assert(_t => _t.IsArray);
        public ComplexType Type
        {
            get => m_type;
            init => m_type = value.Assert(_t => _t.IsArray);
        }

        private ImmutableArray<Expression> m_sizes = Sizes.NonEmpty();
        public ImmutableArray<Expression> Sizes
        {
            get => m_sizes;
            init => m_sizes = Sizes.NonEmpty();
        }
        public int Rank => Sizes.Length;

        #endregion

        #region Override

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            ArrayTypeSyntax type = (ArrayTypeSyntax) Type.EmitNode(_scope, _marker);
            SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = type.RankSpecifiers;
            ArrayRankSpecifierSyntax rank = rankSpecifiers[0].WithSizes(SyntaxFactory.SeparatedList(Sizes.Select(_s => _s.EmitNode(_scope, _marker))));
            rankSpecifiers = SyntaxFactory.List(new[] { rank }.Concat(rankSpecifiers.Skip(1)));
            type = type.WithRankSpecifiers(rankSpecifiers);
            return SyntaxFactory.ArrayCreationExpression(type).Own(_marker, this);
        }
        public override IEnumerable<ComplexTypeOrExpression> Children => new ComplexTypeOrExpression[] { Type }.Concat(Sizes);
        public override string ToString() => $"{GrammarConstants.newKeyword} {Type with { Ranks = default }}[{string.Join(", ", Sizes)}]";

        #endregion

    }

}
