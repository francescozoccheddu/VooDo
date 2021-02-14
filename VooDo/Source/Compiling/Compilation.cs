
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.Problems;

namespace VooDo.Compiling
{

    public sealed partial class Compilation
    {

        public Script Script { get; }
        public CompilationOptions Options { get; }
        public bool Succeded { get; }
        public ImmutableArray<Problem> Problems { get; }

        internal Compilation(Script _script, CompilationOptions _options)
        {
            Script = _script.SetAsRoot(this);
            Options = _options;
            Session session = new Session(this);
            Compiler.Compile(session);
        }


    }

}
