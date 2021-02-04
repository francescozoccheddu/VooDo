
using Microsoft.CodeAnalysis;

using System;

using VooDo.Parsing;

namespace VooDoTB
{
    internal class Program
    {


        private static void Main(string[] _args)
        {
            string code = @"
globals {
int x = 8;
}
y = x;
            ";
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree script = Parser.ParseScript(code);
            Console.WriteLine(script.GetRoot().NormalizeWhitespace().ToFullString());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
