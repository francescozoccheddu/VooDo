using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using System.Collections.Immutable;
using System.IO;
using System.Linq;

using VooDo.AST;
using VooDo.Compilation.Emission;
using VooDo.Errors.Problems;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.Compilation
{

    public sealed class CompiledScript
    {

        private readonly CSharpCompilation m_compilation;
        private readonly Tagger m_tagger;

        internal CompiledScript(CSharpCompilation _compilation, Script _script, Compiler.Options _compilerOptions, ImmutableArray<GlobalPrototype> _globalPrototypes, Tagger _tagger)
        {
            CompilerOptions = _compilerOptions;
            Script = _script;
            GlobalPrototypes = _globalPrototypes;
            m_compilation = _compilation;
            Code = _compilation.SyntaxTrees.Single().GetRoot().NormalizeWhitespace().ToFullString();
            m_tagger = _tagger;
        }

        public Compiler.Options CompilerOptions { get; }
        public ImmutableArray<GlobalPrototype> GlobalPrototypes { get; }
        public Script Script { get; }

        public Loader Load()
        {
            MemoryStream stream = new MemoryStream();
            EmitResult result = m_compilation.Emit(stream);
            if (!result.Success)
            {
                throw result.Diagnostics
                    .SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, m_tagger, Problem.EKind.Emission))
                    .AsThrowable();
            }
            return Loader.FromMemory(stream.ToArray());
        }

        public string Code { get; }

        public void SaveCode(string _file)
            => File.WriteAllText(_file, Code);

        public async void SaveCodeAsync(string _file)
            => await File.WriteAllTextAsync(_file, Code);

        public void SaveLibrary(string _file)
        {
            EmitResult result = m_compilation.Emit(_file);
            if (!result.Success)
            {
                throw result.Diagnostics
                    .SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, m_tagger, Problem.EKind.Emission))
                    .AsThrowable();
            }
        }

    }

}
