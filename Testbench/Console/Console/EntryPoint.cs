using System;

using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;

namespace VooDo.ConsoleTestbench
{
    internal static class EntryPoint
    {


        public static void Run()
        {
            Console.WriteLine(
                new ObjectCreationExpression(
                    ComplexType.Parse("Ciao<int>.Culo[]"),
                    new BinaryExpression(new NameExpression("x"), BinaryExpression.EKind.Add, LiteralExpression.Create(5))).ToString()
                );
        }

    }
}
