
using System;
using System.Collections.Immutable;
using System.ComponentModel;

using VooDo.AST;
using VooDo.Compiling;
using VooDo.Parsing;

namespace VooDo.ConsoleTestbench
{

    public sealed class MyClass1
    {

        public MyClass2 this[int _x] => new MyClass2();

    }


    public sealed class MyClass2 : INotifyPropertyChanged
    {

        public int myField;

        public event PropertyChangedEventHandler? PropertyChanged;
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
            ImmutableArray<Reference> references = Options.Default.References.Add(Reference.FromAssembly(typeof(MyClass1).Assembly));
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
