
using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.Compilation;
using VooDo.Parsing;
using VooDo.Runtime;

namespace VooDo.ConsoleTestbench
{
    public interface Culo : IControllerFactory<bool> { }
    internal static class EntryPoint
    {
        public static void Run()
        {
            string code = @"

using System;

global var x = 7;
int y = 8;
y += x;
$x = null;
y = glob 7 * 4 + 2;
z = (7 + 4) * 2;
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = Reference.GetSystemReferences().Add(Reference.RuntimeReference).Add(Reference.FromAssembly(typeof(Culo).Assembly));
            CompiledScript compilation = Compiler.Compile(script, new Compiler.Options() with { References = references });
            Console.WriteLine(compilation.Code);
        }

    }
}
