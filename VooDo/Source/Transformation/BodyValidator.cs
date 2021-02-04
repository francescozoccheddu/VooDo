using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace VooDo.Transformation
{

    public static class BodyValidator
    {

        private sealed class Walker : CSharpSyntaxWalker
        {

            private readonly List<Diagnostic> m_diagnostics = new List<Diagnostic>();
            public ImmutableArray<Diagnostic> Diagnostics => m_diagnostics.ToImmutableArray();

            private void EmitDiagnostic(SyntaxNode _node)
                => m_diagnostics.Add(DiagnosticFactory.ForbiddenSyntax(_node));

        }

        public static ImmutableArray<Diagnostic> GetSyntaxDiagnostics(SyntaxNode _body)
        {
            if (_body == null)
            {
                throw new ArgumentNullException(nameof(_body));
            }
            Walker walker = new Walker();
            walker.Visit(_body);
            return walker.Diagnostics;
        }

    }

}
