using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Compiling.Transformation;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Compiling
{

    internal static class Compiler
    {

        public const string runtimeReferenceAlias = "VooDoRuntime";
        public const string generatedClassName = "GeneratedProgram";
        public const string globalFieldPrefix = "field_";

        internal static void Compile(Session _session)
        {
            Tagger tagger = new Tagger();
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
                syntax = _script.EmitNode(scope, tagger, _options.References.SelectMany(_r => _r.Aliases).ToImmutableArray(), _options.ReturnType);
                tree = CSharpSyntaxTree.Create(syntax, parseOptions);
                tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, tagger, Problem.EKind.Syntactic)).ThrowErrors();
                syntax = (CompilationUnitSyntax) tree.GetRoot();
                compilation = CSharpCompilation.Create(null, new[] { tree }, _options.References.Select(_r => _r.GetMetadataReference()), compilationOptions);
                semantics = compilation.GetSemanticModel(tree);
            }
            {
                // Global type inference
                CompilationUnitSyntax newSyntax = ImplicitGlobalTypeRewriter.Rewrite(semantics, );
                if (newSyntax != syntax)
                {
                    SyntaxTree newTree = CSharpSyntaxTree.Create(newSyntax, parseOptions);
                    compilation = compilation.ReplaceSyntaxTree(tree, newTree);
                    tree = newTree;
                    tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, tagger, Problem.EKind.Syntactic)).ThrowErrors();
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
            compilation.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, tagger, Problem.EKind.Semantic)).ThrowErrors();
            ImmutableArray<GlobalPrototype> globalPrototypes = scope.GetGlobalDefinitions().Select(_g => _g.Prototype).ToImmutableArray();
        }


    }


}
