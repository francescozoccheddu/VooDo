
using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Parsing;

using roslyn = Microsoft.CodeAnalysis;

namespace VooDo.ConsoleTestbench
{

    public sealed class MyClass1
    {

        public MyClass2 this[int _x] => new MyClass2();

    }


    public sealed class MyClass2
    {

        public int myField;

    }

    public sealed class Hooker : IHookInitializerProvider, IHookInitializer
    {
        Expression IHookInitializer.CreateInitializer() => LiteralExpression.Null;
        IHookInitializer? IHookInitializerProvider.Provide(roslyn::ISymbol _symbol) => this;
    }

    internal static class EntryPoint
    {
        public static void Run()
        {

            string code = @"
var x = new VooDo.ConsoleTestbench.MyClass1();
var y = x[0].myField;
var z = x[1].myField;
            ";
            Script script = Parser.Script(code);
            ImmutableArray<Reference> references = CompilationOptions.Default.References.Add(Reference.FromAssembly(typeof(MyClass1).Assembly));
            Compilation compilation = Compilation.Create(script, CompilationOptions.Default with
            {
                HookInitializerProvider = new Hooker(),
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
