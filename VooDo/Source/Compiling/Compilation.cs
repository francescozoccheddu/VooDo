
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.Compiling
{

    public sealed partial class Compilation
    {

        public Script Script { get; }
        public Options Options { get; }
        public bool Succeded { get; }
        public ImmutableArray<Problem> Problems { get; }
        public ImmutableArray<GlobalPrototype> Globals { get; }

        private readonly CSharpCompilation? m_cSharpCompilation;
        private readonly string? m_cSharpCode;

        public static Compilation Create(Script _script, Options _options)
            => new Compilation(_script, _options);

        public static Compilation SucceedOrThrow(Script _script, Options _options)
        {
            Compilation compilation = Create(_script, _options);
            if (!compilation.Succeded)
            {
                throw compilation.Problems.AsThrowable();
            }
            return compilation;
        }

        private Compilation(Script _script, Options _options)
        {
            Script = _script.SetAsRoot(this);
            Options = _options;
            Session session = new Session(this);
            session.Run();
            Succeded = session.Succeeded;
            Problems = session.GetProblems();
            Globals = session.Globals.EmptyIfDefault().Select(_g => _g.Prototype).ToImmutableArray();
            if (Succeded)
            {
                m_cSharpCompilation = session.CSharpCompilation;
                m_cSharpCode = m_cSharpCompilation?.SyntaxTrees.Single().GetRoot().NormalizeWhitespace().ToFullString();
            }
        }

        public Loader Load()
        {
            Assembly assembly = Assembly.Load(EmitRawAssembly());
            return Loader.FromAssembly(assembly, Options.Namespace, Options.ClassName);
        }

        public byte[] EmitRawAssembly()
        {
            if (!Succeded)
            {
                throw new InvalidOperationException("Compilation did not succeed");
            }
            using MemoryStream stream = new();
            EmitResult result = m_cSharpCompilation!.Emit(stream);
            if (!result.Success)
            {
                throw result.Diagnostics.SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, null, Problem.EKind.Emission)).AsThrowable();
            }
            return stream.ToArray();
        }

        public void SaveLibraryFile(string _file)
        {
            if (!Succeded)
            {
                throw new InvalidOperationException("Compilation did not succeed");
            }
            EmitResult result = m_cSharpCompilation!.Emit(_file);
            if (!result.Success)
            {
                throw result.Diagnostics.SelectNonNull(_d => RoslynProblem.FromDiagnostic(_d, null, Problem.EKind.Emission)).AsThrowable();
            }
        }

        public void SaveCSharpSourceFile(string _file)
        {
            if (!Succeded)
            {
                throw new InvalidOperationException("Compilation did not succeed");
            }
            File.WriteAllText(_file, m_cSharpCode);
        }

        public string GetCSharpSourceCode()
        {
            if (!Succeded)
            {
                throw new InvalidOperationException("Compilation did not succeed");
            }
            return m_cSharpCode!;
        }

    }

}
