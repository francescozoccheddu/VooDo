using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
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
    public static void TestMethod() {
        var v1, v2;
        var x;
        $(v1) = v2;
        v1 = $(v2);
        $(v2) = $(v1);
        x = v1;
        x = nameof(v1);
        $(x) = 3;
    }
}
            ";
            code = code.Replace("$", GlobalVariableAccessTransformer.controllerOfMacro);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = (CompilationUnitSyntax) tree.GetRoot();
            BlockSyntax block = root.DescendantNodes().OfType<BlockSyntax>().First();
            root = root.ReplaceNode(block, SpanTransformer.SetDescendantNodesSourceSpan(block, -block.FullSpan.Start));
            tree = CSharpSyntaxTree.Create(root);
            root = (CompilationUnitSyntax) tree.GetRoot();
            CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree });
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            block = root.DescendantNodes().OfType<BlockSyntax>().First();
            VariableDeclaratorSyntax[] variableDeclarators = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().Take(2).ToArray();
            block = GlobalVariableAccessTransformer.ReplaceDescendantNodesAccess(block, semantics, variableDeclarators);
            Console.WriteLine(block.ToFullString());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
