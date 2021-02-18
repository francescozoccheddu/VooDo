
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
""ciao"" + "" mondo!""
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = CompilationOptions.Default.References.Add(Reference.FromAssembly(typeof(Culo).Assembly));
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
