using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compilation.Transformation;
using VooDo.Utils;

namespace VooDo.Compilation
{

    public static class Compiler
    {

        public const string runtimeReferenceAlias = "VooDoRuntime";
        public const string generatedClassName = "GeneratedProgram";
        public const string globalFieldPrefix = "field_";

        public static void Compile(Script _script, ImmutableArray<Reference> _references, ComplexType? _returnType)
        {
            {
                _references = Reference.Merge(_references);
                int runtimeIndex = _references.IndexOf(Reference.RuntimeReference, Reference.MetadataEqualityComparer);
                if (runtimeIndex < 0)
                {
                    throw new ArgumentException("No runtime reference", nameof(_references));
                }
                if (!_references[runtimeIndex].Aliases.Contains(runtimeReferenceAlias))
                {
                    throw new ArgumentException($"Runtime reference must define '{runtimeReferenceAlias}' alias", nameof(_references));
                }
            }
            Marker marker = new Marker();
            Scope scope = new Scope();
            CompilationUnitSyntax syntax;
            {
                ImmutableArray<Identifier> externAliases = _references
                    .SelectMany(_r => _r.Aliases)
                    .ToImmutableArray();
                if (externAliases.FirstDuplicate(out Identifier? duplicateAlias))
                {
                    throw new ArgumentException($"Duplicate reference alias '{duplicateAlias}'", nameof(_references));
                }
                syntax = _script.EmitNode(scope, marker, externAliases, _returnType);
            }
            SyntaxTree tree;
            CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(false)
                .WithOverflowChecks(true)
                .WithNullableContextOptions(NullableContextOptions.Disable)
                .WithMetadataImportOptions(MetadataImportOptions.Public)
                .WithUsings("System");
            tree = CSharpSyntaxTree.Create(syntax, parseOptions);
            syntax = (CompilationUnitSyntax) tree.GetRoot();
            CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree }, _references.Select(_r => _r.GetMetadataReference()), compilationOptions);
            ImmutableArray<Diagnostic> d = compilation.GetDiagnostics();
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            {
                CompilationUnitSyntax newSyntax = ImplicitGlobalTypeRewriter.Rewrite(semantics, scope.GetGlobalDefinitions());
                if (newSyntax != syntax)
                {
                    SyntaxTree newTree = CSharpSyntaxTree.Create(newSyntax, parseOptions);
                    compilation = compilation.ReplaceSyntaxTree(tree, newTree);
                    tree = newTree;
                    semantics = compilation.GetSemanticModel(tree);
                    syntax = (CompilationUnitSyntax) tree.GetRoot();
                }
            }
        }

    }

}
