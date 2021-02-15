
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Compiling
{

    public sealed partial class Compilation
    {

        public Script Script { get; }
        public CompilationOptions Options { get; }
        public bool Succeded { get; }
        public ImmutableArray<Problem> Problems { get; }
        public ImmutableArray<GlobalPrototype> Globals { get; }
        public string? CSharpCode { get; }

        private readonly CSharpCompilation? m_cSharpCompilation;

        public static Compilation Create(Script _script, CompilationOptions _options)
            => new Compilation(_script, _options);

        private Compilation(Script _script, CompilationOptions _options)
        {
            Script = _script.SetAsRoot(this);
            Options = _options;
            Session session = new Session(this);
            session.Run();
            Succeded = session.Succeeded;
            Problems = session.GetProblems();
            m_cSharpCompilation = session.CSharpCompilation;
            Globals = session.Globals.EmptyIfDefault().Select(_g => _g.Prototype).ToImmutableArray();
            CSharpCode = m_cSharpCompilation?.SyntaxTrees.Single().GetRoot().NormalizeWhitespace().ToFullString();
        }

    }

}
