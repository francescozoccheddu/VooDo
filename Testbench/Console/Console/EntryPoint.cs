
using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.Compiling;
using VooDo.Parsing;
using VooDo.Runtime;

namespace VooDo.ConsoleTestbench
{
    public interface CIao : IControllerFactory<bool> { }
    internal static class EntryPoint
    {
        public static void Run()
        {
            string code = @"
global var x = 7;
const {
    var y = 5, z = 6;
}
bool q = glob (VooDo.ConsoleTestbench.CIao) null;
int p = glob null init 8;
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = CompilationOptions.Default.References.Add(Reference.FromAssembly(typeof(CIao).Assembly));
            Compilation compilation = Compilation.Create(script, CompilationOptions.Default with { References = references });
            if (compilation.Succeded)
            {
                Console.WriteLine(compilation.GetCSharpSourceCode());
            }
            else
            {
                Console.WriteLine(string.Join('\n', compilation.Problems));
            }
        }

    }
}
