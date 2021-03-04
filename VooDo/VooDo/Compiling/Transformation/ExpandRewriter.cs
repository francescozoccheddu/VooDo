using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VooDo.Compiling.Transformation
{

    internal static class ExpandRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            public override SyntaxNode? Visit(SyntaxNode? _node) => _node switch
            {
                BinaryExpressionSyntax
                or PostfixUnaryExpressionSyntax
                or ElementAccessExpressionSyntax
                or CastExpressionSyntax
                or ConditionalExpressionSyntax
                or InvocationExpressionSyntax
                or IsPatternExpressionSyntax
                when _node.Parent
                is ExpressionSyntax
                and not ParenthesizedExpressionSyntax
                and not InvocationExpressionSyntax
                => SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)base.Visit(_node)!),
                _ => base.Visit(_node)
            };

        }

        internal static CompilationUnitSyntax Rewrite(CompilationUnitSyntax _syntax)
            => (CompilationUnitSyntax)new Rewriter().Visit(_syntax)!;

    }

}
