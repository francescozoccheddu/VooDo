using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System;

namespace VooDo.Transformation
{

    public static partial class OriginalSpanRewriter
    {

        private sealed class RelativeRewriter : CSharpSyntaxRewriter
        {

            private readonly int m_offset;

            public RelativeRewriter(int _offset) => m_offset = _offset;

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithOriginalSpan(_node.GetOriginalOrFullSpan().Offset(m_offset));

        }

        private sealed class AbsoluteRewriter : CSharpSyntaxRewriter
        {

            private readonly TextSpan m_span;

            public AbsoluteRewriter(TextSpan _span) => m_span = _span;

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithOriginalSpan(m_span);

        }

        public static TNode RewriteRelative<TNode>(TNode _node, int _offset = 0) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            RelativeRewriter rewriter = new RelativeRewriter(_offset);
            return (TNode) rewriter.Visit(_node);
        }

        public static TNode RewriteAbsolute<TNode>(TNode _node, TextSpan _span) where TNode : SyntaxNode
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
