using System;
using System.IO;
using System.Reflection;

using VooDo.AST;
using VooDo.Compiling;
using VooDo.Parsing;

namespace VooDo.ConsoleTestbench
{

    public class EventHolder
    {

        public event Action Event1;
        public event Action Event2;

    }

    internal static class EntryPoint
    {
        public static void Run()
        {
            string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!;
            string file = Path.Combine(directory, "Test.voodo");
            string source = File.ReadAllText(file);
            Script script = Parser.Script(source, file);
            Compilation compilation = Compilation.SucceedOrThrow(script, Options.Default with
            {
                References = Reference.GetSystemReferences()
                                .Add(Reference.RuntimeReference)
                                .Add(Reference.FromAssembly(Assembly.GetExecutingAssembly()))
            });
            Console.WriteLine(compilation.GetCSharpSourceCode());
        }
    }
}
