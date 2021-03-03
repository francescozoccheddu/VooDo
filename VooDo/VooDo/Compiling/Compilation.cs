
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Compiling.Transformation;
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
        private readonly CompilationUnitSyntax? m_cSharpSyntax;
        private readonly string? m_cSharpCode;

        public static Compilation Create(Script _script, Options _options, CSharpCompilation? _existingCompilation = null)
            => Create(_script, _options, CancellationToken.None, _existingCompilation);

        public static Compilation Create(Script _script, Options _options, CancellationToken _cancellationToken, CSharpCompilation? _existingCompilation = null)
        {
            if (_existingCompilation is not null)
            {
                ImmutableArray<Reference> references =
                    _existingCompilation.References
                    .OfType<PortableExecutableReference>()
                    .Where(_r => _r.FilePath is not null)
                    .Select(_r => Reference.FromFile(_r.FilePath!, _r.Properties.Aliases.Select(_a => new Identifier(_a))))
                    .Concat(_options.References)
                    .ToImmutableArray();
                _options = _options with
                {
                    References = references
                };
            }
            return new Compilation(_script, _options, _existingCompilation, _cancellationToken);
        }

        public static Compilation SucceedOrThrow(Script _script, Options _options, CSharpCompilation? _existingCompilation = null)
            => SucceedOrThrow(_script, _options, CancellationToken.None, _existingCompilation);

        public static Compilation SucceedOrThrow(Script _script, Options _options, CancellationToken _cancellationToken, CSharpCompilation? _existingCompilation = null)
        {
            Compilation compilation = Create(_script, _options, _cancellationToken, _existingCompilation);
            if (!compilation.Succeded)
            {
                throw compilation.Problems.AsThrowable();
            }
            return compilation;
        }

        private Compilation(Script _script, Options _options, CSharpCompilation? _existingCompilation, CancellationToken _cancellationToken)
        {
            Script = _script.SetAsRoot(this);
            Options = _options;
            Session session = new(this, _existingCompilation, _cancellationToken);
            Succeded = session.Succeeded;
            Problems = session.GetProblems();
            Globals = session.Globals.EmptyIfDefault().Select(_g => _g.Prototype).ToImmutableArray();
            if (Succeded)
            {
                m_cSharpCompilation = session.CSharpCompilation;
                m_cSharpSyntax = ExpandRewriter.Rewrite(session.Syntax!);
                m_cSharpCode = m_cSharpSyntax.NormalizeWhitespace().ToFullString();
            }
        }

        public Loader Load()
        {
            Assembly assembly = Assembly.Load(EmitRawAssembly());
            return Loader.FromAssembly(assembly);
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

        public CompilationUnitSyntax GetCSharpSyntax()
        {
            if (!Succeded)
            {
                throw new InvalidOperationException("Compilation did not succeed");
            }
            return m_cSharpSyntax!;
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
