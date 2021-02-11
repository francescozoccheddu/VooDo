using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation.Transformation;
using VooDo.Language.AST;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Compilation
{

    public static class Compiler
    {

        public const string runtimeReferenceExternAlias = "VooDoRuntime";
        public const string generatedClassName = "GeneratedProgram";
        public const string globalFieldPrefix = "field_";

        public static CSharpCompilation Compile(Script _script, ImmutableArray<Reference> _references, ComplexType? _returnType)
        {
            {
                _references = Reference.Merge(_references);
                int runtimeIndex = _references.IndexOf(Reference.RuntimeReference, Reference.MetadataEqualityComparer);
                if (runtimeIndex < 0)
                {
                    throw new InvalidOperationException("No runtime reference");
                }
                if (!_references[runtimeIndex].Aliases.Contains(runtimeReferenceExternAlias))
                {
                    throw new InvalidOperationException($"Runtime reference must define '{runtimeReferenceExternAlias}' alias");
                }
            }
            Marker marker = new Marker();
            Scope scope = new Scope();
            CompilationUnitSyntax syntax;
            SyntaxTree tree;
            CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            {
                ImmutableArray<Identifier> externAliases = _references
                    .SelectMany(_r => _r.Aliases)
                    .DistintToImmutableHashSet()
                    .ToImmutableArray();
                syntax = _script.EmitNode(scope, marker, externAliases, _returnType);
            }
            tree = CSharpSyntaxTree.Create(syntax, parseOptions);
            syntax = (CompilationUnitSyntax) tree.GetRoot();
            CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree }, _references.Select(_r => _r.GetMetadataReference()), compilationOptions);
            ImmutableArray<Diagnostic> d = compilation.GetDiagnostics();
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            {
                IEnumerable<SyntaxToken> globalFields = scope.GetGlobalDefinitions().Select(_g => _g.Identifier);
                syntax = ImplicitGlobalTypeRewriter.Rewrite(semantics, globalFields.ToImmutableHashSet());
                SyntaxTree newTree = CSharpSyntaxTree.Create(syntax, parseOptions);
                compilation = compilation.ReplaceSyntaxTree(tree, newTree);
                tree = newTree;
                semantics = compilation.GetSemanticModel(tree);
            }
            throw new NotImplementedException();
        }

    }

}
