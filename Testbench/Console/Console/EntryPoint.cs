
using System;
using System.Collections.Immutable;
using System.Reflection;

using VooDo.AST;
using VooDo.Compiling;
using VooDo.Parsing;

namespace VooDo.ConsoleTestbench
{



    internal static class EntryPoint
    {
        public static void Run()
        {

            string code = @"
global var x = 4;
int y = 5;
x += 2;
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = Options.Default.References.Add(Reference.FromAssembly(Assembly.GetExecutingAssembly()));
            Compilation compilation = Compilation.Create(script, Options.Default with
            {
                References = references
            });
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
