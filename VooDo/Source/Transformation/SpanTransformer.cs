using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System;

namespace VooDo.Transformation
{

    public static partial class SpanTransformer
    {

        private sealed class SetSourceSpanRewriter : CSharpSyntaxRewriter
        {

            private readonly int m_offset;

            public SetSourceSpanRewriter(int _offset) => m_offset = _offset;

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithSpan(_node.FullSpan.Offset(m_offset));

        }

        private sealed class SetSpanRewriter : CSharpSyntaxRewriter
        {

            private readonly TextSpan m_span;

            public SetSpanRewriter(TextSpan _span) => m_span = _span;

            public override SyntaxNode Visit(SyntaxNode _node)
                => base.Visit(_node)
                ?.WithSpan(m_span);

        }

        public static TNode SetDescendantNodesSourceSpan<TNode>(TNode _node, int _offset = 0) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            SetSourceSpanRewriter rewriter = new SetSourceSpanRewriter(_offset);
            return (TNode) rewriter.Visit(_node);
        }

        public static TNode SetDescendantNodesSpan<TNode>(TNode _node, TextSpan _span) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            SetSpanRewriter rewriter = new SetSpanRewriter(_span);
            return (TNode) rewriter.Visit(_node);
        }

    }

}
