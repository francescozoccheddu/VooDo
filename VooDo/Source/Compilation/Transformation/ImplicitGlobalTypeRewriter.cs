using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;

namespace VooDo.Compilation.Transformation
{

    internal static class ImplicitGlobalTypeRewriter
    {

        internal static CompilationUnitSyntax Rewrite(SemanticModel _semantics, ImmutableHashSet<SyntaxToken> _globalFields)
        {
            throw new Exception();
        }

    }


}
