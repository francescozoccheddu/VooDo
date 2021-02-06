

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VooDo.Factory;

namespace VooDo.Transformation
{

    public static partial class OriginRewriter
    {

        private sealed class RelativeRewriter : CSharpSyntaxRewriter
        {

            public override SyntaxNode? Visit(SyntaxNode? _node)
                => base.Visit(_node)
                ?.WithOrigin(_node!.TryGetOrigin() ?? OriginExtensions.FromSpan(_node!.Span));

            public override SyntaxToken VisitToken(SyntaxToken _token)
                => base.VisitToken(_token)
                .WithOrigin(_token.TryGetOrigin() ?? OriginExtensions.FromSpan(_token.Span));

        }

        private sealed class AbsoluteRewriter : CSharpSyntaxRewriter
        {

            private readonly Origin? m_origin;

            public AbsoluteRewriter(Origin? _origin) => m_origin = _origin;

            public override SyntaxNode? Visit(SyntaxNode? _node)
                => base.Visit(_node)
                ?.WithOrigin(m_origin);

            public override SyntaxToken VisitToken(SyntaxToken _token)
                => base.VisitToken(_token)
                .WithOrigin(m_origin);

        }

        public static TNode RewriteFromFullSpan<TNode>(TNode _node) where TNode : SyntaxNode
            => (TNode) new RelativeRewriter().Visit(_node)!;

        public static TNode RewriteAbsolute<TNode>(TNode _node, Origin? _origin) where TNode : SyntaxNode
            => (TNode) new AbsoluteRewriter(_origin).Visit(_node)!;

    }

}
