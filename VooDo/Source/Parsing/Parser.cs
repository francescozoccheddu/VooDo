using Antlr4.Runtime;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Parsing.Generated;

namespace VooDo.Parsing
{

    public static class Parser
    {

        private static VooDoParser MakeParser(string _script)
        {
            VooDoLexer lexer = new VooDoLexer(new AntlrInputStream(_script));
            return new VooDoParser(new CommonTokenStream(lexer));
        }

        private static CSharpSyntaxTree Parse(ParserRuleContext _rule)
        {
            CompilationUnitSyntax root = (CompilationUnitSyntax) new Visitor().Visit(_rule);
            return (CSharpSyntaxTree) CSharpSyntaxTree.Create(root);
        }

        public static CSharpSyntaxTree ParseScript(string _script) => Parse(MakeParser(_script).script());

        public static CSharpSyntaxTree ParseInlineScript(string _script) => Parse(MakeParser(_script).inlineScript());

    }

}
