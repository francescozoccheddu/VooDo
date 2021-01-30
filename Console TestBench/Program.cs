using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Transformation;

namespace VooDoTB
{
    internal class Program
    {


        private static void Main(string[] _args)
        {
            string code = @"
public static class TestClass {
    public event System.Action MyEvent1;
    public event System.Action MyEvent2;
    public static void TestMethod() {
        var y = this.MyEvent1;
        var z = this.MyEvent2;
        var q = this.MyEvent2(out int p1, ref p2);
        z = this.MyEvent1(out int p1);
        q = this.MyEvent1(out int p1, ref p2);
    }
}
            ";
            code = code.Replace("$", Identifiers.controllerOfMacro);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = (CompilationUnitSyntax) tree.GetRoot();
            BlockSyntax block = root.DescendantNodes().OfType<BlockSyntax>().First();
            root = root.ReplaceNode(block, OriginalSpanRewriter.RewriteRelative(block, -block.FullSpan.Start));
            tree = CSharpSyntaxTree.Create(root);
            root = (CompilationUnitSyntax) tree.GetRoot();
            CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree });
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            block = root.DescendantNodes().OfType<BlockSyntax>().First();
            block = EventAccessRewriter.Rewrite(block, semantics, out ImmutableArray<IEventSymbol> symbols, out ImmutableHashSet<GetEventOverload> overloads);
            //VariableDeclaratorSyntax[] variableDeclarators = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().Take(2).ToArray();
            //block = GlobalVariableAccessRewriter.Rewrite(block, semantics, variableDeclarators);
            Console.WriteLine(block.ToFullString());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
