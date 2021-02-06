
using System;

using VooDo.Factory;

namespace VooDoTB
{
    internal class Program
    {

        private static void Main(string[] _args)
        {
            string code = @"
int VooDo_VooDo;
int x = 7;
global float y = 8;
global var z = 8;
            ";
            ScriptSource.FromScript(code)
                .WithAdditionalReferences(Reference.FromAssembly(typeof(object).Assembly))
                .Compile();
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
