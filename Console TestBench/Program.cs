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
    public event System.Action<int> MyEvent1;
    public event System.Action<int, float> MyEvent2;
    public static void TestMethod() {
        int vi;
        float vf;
        _ = this.MyEvent1;
        _ = this.MyEvent2;
        _ = this.MyEvent2(out int p1, ref vf);
        _ = this.MyEvent2(out vi);
        _ = this.MyEvent1();
        _ = this.MyEvent1(out var p1);
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
            PortableExecutableReference[] references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
            CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree }, references);
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            block = root.DescendantNodes().OfType<BlockSyntax>().First();
            block = EventCatcherRewriter.Rewrite(block, semantics, out ImmutableArray<IEventSymbol> symbols, out ImmutableHashSet<GetEventOverload> overloads);
            //VariableDeclaratorSyntax[] variableDeclarators = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().Take(2).ToArray();
            //block = GlobalVariableAccessRewriter.Rewrite(block, semantics, variableDeclarators);
            Console.WriteLine(block.ToFullString());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
