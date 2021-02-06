using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;

using VooDo.Factory;

namespace VooDo.Transformation
{

    internal static class GlobalVariableDeclarationRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private readonly SemanticModel m_semantics;

            public Rewriter(SemanticModel _semantics)
            {
                m_semantics = _semantics;
            }

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax _node)
            {
                TypeInfo type = m_semantics.GetTypeInfo(_node.Type);
                return base.VisitVariableDeclaration(_node);
            }

        }

        internal static CompilationUnitSyntax Rewrite(SemanticModel _semantics, out ImmutableArray<Global> _globals)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            Rewriter rewriter = new Rewriter(_semantics);
            SyntaxNode newRoot = rewriter.Visit(_semantics.SyntaxTree.GetRoot());
            return (CompilationUnitSyntax) newRoot;
        }

    }

}
