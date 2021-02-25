﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Compiling.Transformation;
using VooDo.Problems;
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

        internal Tagger Tagger { get; } = new Tagger();

        internal SemanticModel? Semantics { get; private set; }

        internal ImmutableArray<GlobalDefinition> Globals { get; private set; }

        internal CSharpCompilation? CSharpCompilation { get; private set; }

        internal CompilationUnitSyntax? Syntax { get; private set; }

        internal SyntaxTree? Tree { get; private set; }

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

        internal void Run(CSharpCompilation? _existingCompilation = null)
        {
            try
            {
                if (_existingCompilation is null)
                {
                    InitializeCompilation();
                }
                else
                {
                    InitializeFromExistingCompilation(_existingCompilation);
                }
                RunUnhandled();
            }
            catch (VooDoException e)
            {
                AddProblems(e.Problems);
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
            CSharpCompilation!.GetDiagnostics()
                .Where(_d => _d.Location.SourceTree is null || _d.Location.SourceTree == Tree)
                .SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Semantic))
                .ThrowErrors();
        }

        private void ReplaceTree(CompilationUnitSyntax _syntax)
        {
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
                Tree.GetDiagnostics().SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, Tagger, Problem.EKind.Syntactic)).ThrowErrors();
                Syntax = (CompilationUnitSyntax)Tree.GetRoot();
            }
        }

        private void RunUnhandled()
        {
            Compilation.Script.GetSyntaxProblems().ThrowErrors();
            // Emission
            CompilationUnitSyntax originalSyntax;
            (originalSyntax, Globals) = Emitter.Emit(Compilation.Script, this);
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
