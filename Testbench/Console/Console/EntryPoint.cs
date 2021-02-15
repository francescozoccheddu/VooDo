
using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.Compiling;
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
global var x = 7;
global {
    var y = 5, z = 6;
}
bool q = glob (VooDo.ConsoleTestbench.Culo) null;
int p = glob null init 5;
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = CompilationOptions.Default.References.Add(Reference.FromAssembly(typeof(Culo).Assembly));
            Compilation compilation = Compilation.Create(script, new CompilationOptions() with { References = references });
            if (compilation.Succeded)
            {
                Console.WriteLine(compilation.CSharpCode);
            }
            else
            {
                Console.WriteLine(string.Join('\n', compilation.Problems));
            }
        }

    }
}
