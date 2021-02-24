using System;

using VooDo.Parsing;

namespace VooDo.ConsoleTestbench
{

    internal static class EntryPoint
    {
        public static void Run()
        {
            Console.WriteLine(Parser.Script("var x = 7q"));
        }
    }
}
