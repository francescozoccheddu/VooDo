
using Antlr4.Runtime;

using VooDo.Parsing.Generated;
using VooDo.AST.Statements;

namespace VooDo.Parsing
{

    public static class Parser
    {

        public static Stat Parse(string _script)
        {
            VooDoLexer lexer = new VooDoLexer(new AntlrInputStream(_script));
            VooDoParser parser = new VooDoParser(new CommonTokenStream(lexer));
            return (Stat) new Visitor().Visit(parser.stat());
        }

    }

}
