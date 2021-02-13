
using System;

using VooDo.AST;
using VooDo.Compilation;
using VooDo.Parsing;
using VooDo.Runtime;

namespace VooDo.ConsoleTestbench
{
    public interface Culo : IControllerFactory<bool> { }
    internal static class EntryPoint
    {
        public static void Run()
        {
            string code = @"

using System;

global var x = 7;
int y = 8;
y += x;
$x = null;
y = glob 7 * 4 + 2;
z = (7 + 4) * 2;
            ";
            // using System; var x = 8; x += 5 + y;
            Script script = Parser.Script(code);
            Console.WriteLine();
            Console.WriteLine("----- BFS -----");
            Console.WriteLine(string.Join(" ; ", script.DescendantNodesAndSelf(Tree.ETraversal.BreadthFirst)));

            Console.WriteLine();
            Console.WriteLine("----- PreDFS -----");
            Console.WriteLine(string.Join(" ; ", script.DescendantNodesAndSelf(Tree.ETraversal.PreDepthFirst)));

            Console.WriteLine();
            Console.WriteLine("----- PostDFS -----");
            Console.WriteLine(string.Join(" ; ", script.DescendantNodesAndSelf(Tree.ETraversal.PostDepthFirst)));
            return;
            Compiler.Compile(script, Reference.GetSystemReferences().Add(Reference.RuntimeReference).Add(Reference.FromAssembly(typeof(Culo).Assembly)), null);
        }

    }
}
