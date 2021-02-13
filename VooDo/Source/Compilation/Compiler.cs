using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compilation.Emission;
using VooDo.Compilation.Transformation;
using VooDo.Errors;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.Compilation
{

    public static class Compiler
    {

        public sealed record Options
        {

            public static Options Default { get; } = new Options(null, Reference.GetSystemReferences().Add(Reference.RuntimeReference));

            public Options(ComplexType? _returnType = null, ImmutableArray<Reference> _references = default)
            {
                ReturnType = _returnType;
                References = _references;
            }

            public ComplexType? ReturnType { get; init; }

            private ImmutableArray<Reference> m_references;
            public ImmutableArray<Reference> References
            {
                get => m_references;
                set
                {
                    if (value.IsDefault)
                    {
                        m_references = Default.References;
                    }
                    else
                    {
                        m_references = Reference.Merge(value);
                        int runtimeIndex = m_references.IndexOf(Reference.RuntimeReference, Reference.MetadataEqualityComparer);
                        if (runtimeIndex < 0)
                        {
                            throw new CompilerOptionsException("No runtime reference", nameof(References));
                        }
                        if (!m_references[runtimeIndex].Aliases.Contains(runtimeReferenceAlias))
                        {
                            throw new CompilerOptionsException($"Runtime reference must define '{runtimeReferenceAlias}' alias", nameof(References));
                        }
                        if (m_references.SelectMany(_r => _r.Aliases).FirstDuplicate(out Identifier? duplicateAlias))
                        {
                            throw new CompilerOptionsException($"Duplicate reference alias '{duplicateAlias}'", nameof(References));
                        }
                    }
                }
            }

        }

        public const string runtimeReferenceAlias = "VooDoRuntime";
        public const string generatedClassName = "GeneratedProgram";
        public const string globalFieldPrefix = "field_";

        public static CompiledScript Compile(Script _script, Options _options)
        {
            Marker marker = new Marker();
            Scope scope = new Scope();
            CompilationUnitSyntax syntax;
            SyntaxTree tree;
            CSharpCompilation compilation;
            SemanticModel semantics;
            CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(false)
                .WithOverflowChecks(true)
                .WithNullableContextOptions(NullableContextOptions.Disable)
                .WithMetadataImportOptions(MetadataImportOptions.Public);
            {
                // Emission
                syntax = _script.EmitNode(scope, marker, _options.References.SelectMany(_r => _r.Aliases).ToImmutableArray(), _options.ReturnType);
                tree = CSharpSyntaxTree.Create(syntax, parseOptions);
                tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, marker, Problem.EKind.Syntactic)).ThrowErrors();
                syntax = (CompilationUnitSyntax) tree.GetRoot();
                compilation = CSharpCompilation.Create(null, new[] { tree }, _options.References.Select(_r => _r.GetMetadataReference()), compilationOptions);
                semantics = compilation.GetSemanticModel(tree);
            }
            {
                // Global type inference
                CompilationUnitSyntax newSyntax = ImplicitGlobalTypeRewriter.Rewrite(semantics, scope.GetGlobalDefinitions());
                if (newSyntax != syntax)
                {
                    SyntaxTree newTree = CSharpSyntaxTree.Create(newSyntax, parseOptions);
                    compilation = compilation.ReplaceSyntaxTree(tree, newTree);
                    tree = newTree;
                    tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, marker, Problem.EKind.Syntactic)).ThrowErrors();
                    semantics = compilation.GetSemanticModel(tree);
                    syntax = (CompilationUnitSyntax) tree.GetRoot();
                }
            }
            {
                // Events
            }
            {
                // Hooks
            }
            compilation.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, marker, Problem.EKind.Semantic)).ThrowErrors();
            ImmutableArray<GlobalPrototype> globalPrototypes = scope.GetGlobalDefinitions().Select(_g => _g.Prototype).ToImmutableArray();
            return new CompiledScript(compilation, _script, _options, globalPrototypes, marker);
        }

    }

}
