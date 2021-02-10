using System;
using System.Collections.Immutable;

using VooDo.Language.AST;
using VooDo.Language.AST.Directives;
using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;
using VooDo.Language.AST.Statements;

namespace VooDo.ConsoleTestbench
{
    internal static class EntryPoint
    {


        public static void Run()
        {
            // using System; var x = 8; x += 5 + y;
            Script code = new Script(
                new UsingDirective[] { new UsingNamespaceDirective(null, "System") }.ToImmutableArray(),
                new BlockStatement(new Statement[]
                {
                    new DeclarationStatement(
                        ComplexTypeOrVar.Var,
                        new DeclarationStatement.Declarator[] {
                            new DeclarationStatement.Declarator(
                                "x",
                                LiteralExpression.Create(8))
                        }.ToImmutableArray()),
                    new AssignmentStatement(
                        new NameExpression(
                            false,
                            "x"),
                        AssignmentStatement.EKind.Add,
                        new BinaryExpression(
                            LiteralExpression.Create(5),
                            BinaryExpression.EKind.Add,
                            new NameExpression(
                                false,
                                "y")
                        )
                    )
                }.ToImmutableArray()
            ));
            Console.WriteLine(code);
        }

    }
}
