using System.Collections.Immutable;

using VooDo.Compilation;
using VooDo.Language.AST;
using VooDo.Language.AST.Directives;
using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Names;
using VooDo.Language.AST.Statements;
using VooDo.Runtime;

namespace VooDo.ConsoleTestbench
{
    public interface Culo : IControllerFactory<bool> { }
    internal static class EntryPoint
    {


        public static void Run()
        {
            // using System; var x = 8; x += 5 + y;
            Script script = new Script(
                new UsingDirective[] { new UsingNamespaceDirective(null, "System") }.ToImmutableArray(),
                new BlockStatement(new Statement[]
                {
                    new GlobalStatement(
                        new []
                        {
                            new DeclarationStatement(
                                ComplexTypeOrVar.Parse("int"),
                                new DeclarationStatement.Declarator[] {
                                    new DeclarationStatement.Declarator(
                                        "x",
                                        LiteralExpression.Create(8))
                            }.ToImmutableArray())
                        }.ToImmutableArray()),
                    new AssignmentStatement(
                        new NameExpression(
                            false,
                            "x"),
                        AssignmentStatement.EKind.Add,
                        new BinaryExpression(
                            LiteralExpression.Create("ciao\n\t"),
                            BinaryExpression.EKind.Add,
                            new GlobalExpression(
                                new CastExpression(
                                    QualifiedType.FromType<Culo>(),
                                    LiteralExpression.Null),
                                LiteralExpression.False)
                        )
                    )
                }.ToImmutableArray()
            ));
            Compiler.Compile(script, Reference.GetSystemReferences().Add(Reference.RuntimeReference).Add(Reference.FromAssembly(typeof(Culo).Assembly)), null);
        }

    }
}
