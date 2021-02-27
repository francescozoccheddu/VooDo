using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Compiling.Transformation;
using VooDo.Problems;
using VooDo.Runtime.Implementation;
using VooDo.Utils;

using static VooDo.Compiling.Emission.Scope;

namespace VooDo.Compiling
{

    internal sealed class Session
    {

        private static readonly CSharpParseOptions s_parseOptions
            = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);

        private static readonly CSharpCompilationOptions s_compilationOptions
            = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(false)
                .WithOverflowChecks(true)
                .WithNullableContextOptions(NullableContextOptions.Disable)
                .WithMetadataImportOptions(MetadataImportOptions.Public);

        private readonly List<Problem> m_problems = new List<Problem>();

        internal Compilation Compilation { get; }

        public CancellationToken CancellationToken { get; }

        internal Tagger Tagger { get; } = new Tagger();

        internal SemanticModel Semantics { get; private set; }

        internal ImmutableArray<GlobalDefinition> Globals { get; private set; }

        internal CSharpCompilation CSharpCompilation { get; private set; }

        internal CompilationUnitSyntax Syntax { get; private set; }

        internal ClassDeclarationSyntax Class { get; private set; }

        internal SyntaxTree Tree { get; private set; }

        internal bool Succeeded { get; private set; }

        internal Identifier RuntimeAlias { get; private set; }

        internal MetadataReference RuntimeReference { get; private set; }

        internal void AddProblem(Problem _problem)
            => m_problems.Add(_problem);

        internal void AddProblems(IEnumerable<Problem> _problems)
            => m_problems.AddRange(_problems);

        internal ImmutableArray<Problem> GetProblems()
            => m_problems.ToImmutableArray();

        internal Session(Compilation _compilation, CSharpCompilation? _existingCompilation, CancellationToken _cancellationToken)
        {
            Compilation = _compilation;
            CancellationToken = _cancellationToken;
            RuntimeAlias = null!;
            Semantics = null!;
            CSharpCompilation = null!;
            Tree = null!;
            Syntax = null!;
            RuntimeReference = null!;
            Class = null!;
            Run(_existingCompilation);
        }


        private void InitializeRuntimeSymbol()
        {
            MetadataReference reference = ReferenceFinder.FindByType(typeof(Program), CSharpCompilation).FirstOrDefault();
            if (reference is null)
            {
                throw new NoRuntimeReferenceProblem().AsThrowable();
            }
            RuntimeReference = reference;
            RuntimeAlias = ReferenceFinder.GetAlias(reference);
        }

        private void Run(CSharpCompilation? _existingCompilation = null)
        {
            try
            {
                EnsureNotCanceled();
                if (_existingCompilation is null)
                {
                    InitializeCompilation();
                }
                else
                {
                    InitializeFromExistingCompilation(_existingCompilation);
                }
                InitializeRuntimeSymbol();
                RunUnhandled();
            }
            catch (VooDoException e)
            {
                AddProblems(e.Problems);
            }
            catch
            {
                if (CancellationToken.IsCancellationRequested && !m_problems.Any(_p => _p is CanceledProblem))
                {
                    AddProblem(new CanceledProblem());
                }
                else
                {
                    throw;
                }
            }
        }

        private void InitializeCompilation()
        {
            CSharpCompilation = CSharpCompilation.Create(null, null, Compilation.Options.References.Select(_r => _r.GetMetadataReference()), s_compilationOptions);
        }

        private void InitializeFromExistingCompilation(CSharpCompilation _compilation)
        {
            ImmutableArray<Reference> references =
                _compilation.References
                .OfType<PortableExecutableReference>()
                .Where(_r => _r.FilePath is not null)
                .Select(_r => Reference.FromFile(_r.FilePath!, _r.Properties.Aliases.Select(_a => new Identifier(_a))))
                .Concat(Compilation.Options.References)
                .ToImmutableArray();
            references = Reference.Merge(references);
            CSharpCompilation = _compilation.WithReferences(references.Select(_r => _r.GetMetadataReference()));
        }

        private void ThrowCompilationErrors()
        {
            EnsureNotCanceled();
            CSharpCompilation!.GetDiagnostics()
                .Where(_d => _d.Location.SourceTree is null || _d.Location.SourceTree == Tree)
                .SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Semantic))
                .ThrowErrors();
        }

        private void ReplaceTree(CompilationUnitSyntax _syntax)
        {
            EnsureNotCanceled();
            if (_syntax != Syntax)
            {
                SyntaxTree newTree = CSharpSyntaxTree.Create(_syntax, s_parseOptions);
                if (CSharpCompilation is not null)
                {
                    if (Tree is not null)
                    {
                        CSharpCompilation = CSharpCompilation.ReplaceSyntaxTree(Tree, newTree);
                    }
                    else
                    {
                        CSharpCompilation = CSharpCompilation.AddSyntaxTrees(newTree);
                    }
                    Semantics = CSharpCompilation.GetSemanticModel(newTree);
                }
                Tree = newTree;
                Tree.GetDiagnostics(CancellationToken).SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Syntactic)).ThrowErrors();
                Syntax = (CompilationUnitSyntax)Tree.GetRoot();
                MemberDeclarationSyntax classDeclaration = Syntax.Members[0];
                if (Compilation.Options.Namespace is not null)
                {
                    classDeclaration = ((NamespaceDeclarationSyntax)classDeclaration).Members[0];
                }
                if (Compilation.Options.ContainingClass is not null)
                {
                    classDeclaration = ((ClassDeclarationSyntax)classDeclaration).Members[0];
                }
                Class = (ClassDeclarationSyntax)classDeclaration;
            }
        }

        public void EnsureNotCanceled()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                throw new CanceledProblem().AsThrowable();
            }
        }

        private void RunUnhandled()
        {
            EnsureNotCanceled();
            Compilation.Script.GetSyntaxProblems().ThrowErrors();
            // Emission
            CompilationUnitSyntax originalSyntax;
            (originalSyntax, Globals) = Emitter.Emit(Compilation.Script, this, RuntimeAlias);
            ReplaceTree(originalSyntax);
            // Global type inference
            ReplaceTree(ImplicitGlobalTypeRewriter.Rewrite(this));
            // Events
            ThrowCompilationErrors();
            // Hooks
            ReplaceTree(HookRewriter.Rewrite(this));
            ThrowCompilationErrors();
            Succeeded = true;
        }

    }

}
