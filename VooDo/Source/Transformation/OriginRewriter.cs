using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System;

namespace VooDo.Transformation
{

    public static partial class OriginRewriter
    {

        private sealed class RelativeRewriter : CSharpSyntaxRewriter
        {

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithOriginalSpan(_node.GetOriginalOrFullSpan());

            public override SyntaxToken VisitToken(SyntaxToken _token)
                => base.VisitToken(_token)
                .WithOriginalSpan(_token.GetOriginalOrFullSpan());

        }

        private sealed class AbsoluteRewriter : CSharpSyntaxRewriter
        {

            private readonly TextSpan? m_span;

            public AbsoluteRewriter(TextSpan? _span) => m_span = _span;

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithOriginalSpan(m_span);

            public override SyntaxToken VisitToken(SyntaxToken _token)
                => base.VisitToken(_token)
                .WithOriginalSpan(m_span);

        }

        public static TNode RewriteFromFullSpan<TNode>(TNode _node) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            RelativeRewriter rewriter = new RelativeRewriter();
            return (TNode) rewriter.Visit(_node);
        }

        public static TNode RewriteAbsolute<TNode>(TNode _node, TextSpan? _span) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            AbsoluteRewriter rewriter = new AbsoluteRewriter(_span);
            return (TNode) rewriter.Visit(_node);
        }

    }

}
