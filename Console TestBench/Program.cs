using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Parsing;

namespace VooDoTB
{
    internal class Program
    {

        private static void Main(string[] _args)
        {
            string code = @"
if (x is int x)
{}
if (x is int _)
{}
x.ciao(out int a, out int _);
            ";
            CompilationUnitSyntax script = Parser.ParseAnyScript(code);
            Console.WriteLine(script.NormalizeWhitespace().ToFullString());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
