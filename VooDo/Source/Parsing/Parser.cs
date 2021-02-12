using Antlr4.Runtime;

using System;

using VooDo.AST;
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

        private static Script Parse(ParserRuleContext _rule)
            => (Script) new Visitor().Visit(_rule);

        public static Script Script(string _script) => Parse(MakeParser(_script).script());

        public static Script Statement(string _script) => Parse(MakeParser(_script).script());

        public static Script UsingDirective(string _script) => Parse(MakeParser(_script).script());

        public static Script Expression(string _script) => Parse(MakeParser(_script).script());

        public static Script ComplexTypeOrVar(string _script) => Parse(MakeParser(_script).script());

        public static Script ComplexType(string _script) => Parse(MakeParser(_script).script());

        public static Script QualifiedType(string _script) => Parse(MakeParser(_script).script());

        public static Script TupleType(string _script) => Parse(MakeParser(_script).script());

        public static Script Namespace(string _script) => Parse(MakeParser(_script).script());

        public static Script SimpleType(string _script) => Parse(MakeParser(_script).script());

        public static TNodeOrIdentifier NodeOrIdentifier<TNodeOrIdentifier>(string _script) where TNodeOrIdentifier : NodeOrIdentifier
            => throw new NotImplementedException();

    }

}
