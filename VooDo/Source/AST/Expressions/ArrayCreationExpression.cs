using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ArrayCreationExpression : Expression
    {

        #region Members

        public ArrayCreationExpression(ComplexType _type, ImmutableArray<Expression> _sizes)
        {
            m_type = Type = _type;
            m_sizes = Sizes = _sizes;
        }

        private ComplexType m_type;
        public ComplexType Type
        {
            get => m_type;
            init
            {
                if (!value.IsArray)
                {
                    throw new ChildSyntaxError(this, value, "Array creation type must be array").AsThrowable();
                }
                m_type = value;
            }
        }
        private ImmutableArray<Expression> m_sizes;
        public ImmutableArray<Expression> Sizes
        {
            get => m_sizes;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "Array creation sizes array cannot be empty").AsThrowable();
                }
                m_sizes = value.EmptyIfDefault();
            }
        }
        public int Rank => Sizes.Length;

        #endregion

        #region Override

        protected override IEnumerable<Problem> GetSelfSyntaxProblems()
        {
            if (Type.Ranks[0].Rank != Sizes.Length)
            {
                yield return new SyntaxError(this, "Type first rank does not match the number of sizes");
            }
        }

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

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

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            ArrayTypeSyntax type = (ArrayTypeSyntax) Type.EmitNode(_scope, _marker);
            SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = type.RankSpecifiers;
            ArrayRankSpecifierSyntax rank = rankSpecifiers[0].WithSizes(Sizes.Select(_s => _s.EmitNode(_scope, _marker)).ToSeparatedList());
            rankSpecifiers = new[] { rank }.Concat(rankSpecifiers.Skip(1)).ToSyntaxList();
            type = type.WithRankSpecifiers(rankSpecifiers);
            return SyntaxFactory.ArrayCreationExpression(type).Own(_marker, this);
        }
        public override IEnumerable<ComplexTypeOrExpression> Children => new ComplexTypeOrExpression[] { Type }.Concat(Sizes);
        public override string ToString() => $"{GrammarConstants.newKeyword} {Type with { Ranks = default }}[{string.Join(", ", Sizes)}]";

        #endregion

    }

}
