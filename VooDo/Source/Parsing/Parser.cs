using Antlr4.Runtime;

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

        private static CompilationUnitSyntax Parse(ParserRuleContext _rule)
            => (CompilationUnitSyntax) new Visitor().Visit(_rule);

        public static CompilationUnitSyntax ParseScript(string _script) => Parse(MakeParser(_script).script());

        public static CompilationUnitSyntax ParseInlineScript(string _script) => Parse(MakeParser(_script).inlineScript());

        public static CompilationUnitSyntax ParseAnyScript(string _script) => Parse(MakeParser(_script).anyScript());

    }

}
