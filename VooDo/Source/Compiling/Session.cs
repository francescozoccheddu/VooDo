using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Compiling.Transformation;
using VooDo.Problems;
using VooDo.Utils;

using static VooDo.Compiling.Emission.Scope;

namespace VooDo.Compiling
{

    internal sealed class Session
    {

        internal const string runtimeReferenceAlias = "VooDoRuntime";
        internal const string generatedClassName = "GeneratedProgram";
        internal const string globalFieldPrefix = "field_";

        private readonly List<Problem> m_problems = new List<Problem>();

        internal Compilation Compilation { get; }

        internal Tagger Tagger { get; } = new Tagger();

        internal SemanticModel? Semantics { get; private set; }

        internal ImmutableArray<GlobalDefinition> Globals { get; private set; }

        internal CSharpCompilation? CSharpCompilation { get; private set; }

        internal CompilationUnitSyntax? Syntax { get; private set; }

        internal bool Succeeded { get; private set; }

        internal void AddProblem(Problem _problem)
            => m_problems.Add(_problem);

        internal void AddProblems(IEnumerable<Problem> _problems)
            => m_problems.AddRange(_problems);

        internal ImmutableArray<Problem> GetProblems()
            => m_problems.ToImmutableArray();

        internal Session(Compilation _compilation)
        {
            Compilation = _compilation;
        }

        internal void Run()
        {
            try
            {
                RunUnhandled();
            }
            catch (VooDoException e)
            {
                AddProblems(e.Problems);
            }
        }

        private void RunUnhandled()
        {
            Compilation.Script.GetSyntaxProblems().ThrowErrors();
            SyntaxTree tree;
            CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(false)
                .WithOverflowChecks(true)
                .WithNullableContextOptions(NullableContextOptions.Disable)
                .WithMetadataImportOptions(MetadataImportOptions.Public);
            {
                // Emission
                (Syntax, Globals) = Compilation.Script.EmitNode(this);
                tree = CSharpSyntaxTree.Create(Syntax, parseOptions);
                tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Syntactic)).ThrowErrors();
                Syntax = (CompilationUnitSyntax) tree.GetRoot();
                CSharpCompilation = CSharpCompilation.Create(null, new[] { tree }, Compilation.Options.References.Select(_r => _r.GetMetadataReference()), compilationOptions);
                Semantics = CSharpCompilation.GetSemanticModel(tree);
            }
            {
                // Global type inference
                CompilationUnitSyntax newSyntax = ImplicitGlobalTypeRewriter.Rewrite(this);
                if (newSyntax != Syntax)
                {
                    SyntaxTree newTree = CSharpSyntaxTree.Create(newSyntax, parseOptions);
                    CSharpCompilation = CSharpCompilation.ReplaceSyntaxTree(tree, newTree);
                    tree = newTree;
                    tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Syntactic)).ThrowErrors();
                    Semantics = CSharpCompilation.GetSemanticModel(tree);
                    Syntax = (CompilationUnitSyntax) tree.GetRoot();
                }
            }
            {
                // Events
            }
            {
                // Hooks
            }
            CSharpCompilation.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Semantic)).ThrowErrors();
            Succeeded = true;
        }

    }

}
