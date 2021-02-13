using Antlr4.Runtime;

using System;
using System.IO;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Parsing.Generated;

namespace VooDo.Parsing
{

    public static class Parser
    {

        private sealed class ErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
        {

            public static ErrorListener Instance { get; } = new ErrorListener();

            private ErrorListener() { }

            public void SyntaxError(TextWriter _output, IRecognizer _recognizer, int _offendingSymbol, int _line, int _charPositionInLine, string _msg, RecognitionException _e)
                => throw new NotImplementedException();

            public void SyntaxError(TextWriter _output, IRecognizer _recognizer, IToken _offendingSymbol, int _line, int _charPositionInLine, string _msg, RecognitionException _e)
                => throw new NotImplementedException();
        }

        private static VooDoParser MakeParser(string _script)
        {
            VooDoLexer lexer = new VooDoLexer(new AntlrInputStream(_script));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(ErrorListener.Instance);
            VooDoParser parser = new VooDoParser(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ErrorListener.Instance);
            return parser;
        }

        private static TNodeOrIdentifier Parse<TNodeOrIdentifier>(string _source, Func<VooDoParser, ParserRuleContext> _ruleProvider) where TNodeOrIdentifier : NodeOrIdentifier
            => (TNodeOrIdentifier) new Visitor(_source).Visit(_ruleProvider(MakeParser(_source)));

        // Script
        public static Script Script(string _source) => Parse<Script>(_source, _p => _p.script_Greedy());
        // Directives
        public static UsingDirective UsingDirective(string _source) => Parse<UsingDirective>(_source, _p => _p.usingDirective_Greedy());
        // Expressions
        public static Expression Expression(string _source) => Parse<Expression>(_source, _p => _p.expression_Greedy());
        // Names
        public static ComplexType ComplexType(string _source) => Parse<ComplexType>(_source, _p => _p.complexType_Greedy());
        public static SimpleType SimpleType(string _source) => Parse<SimpleType>(_source, _p => _p.simpleType_Greedy());
        public static QualifiedType QualifiedType(string _source) => Parse<QualifiedType>(_source, _p => _p.qualifiedType_Greedy());
        public static TupleType TupleType(string _source) => Parse<TupleType>(_source, _p => _p.tupleType_Greedy());
        public static Identifier Identifier(string _source) => Parse<Identifier>(_source, _p => _p.identifier_Greedy());
        public static Namespace Namespace(string _source) => Parse<Namespace>(_source, _p => _p.namespace_Greedy());
        public static ComplexTypeOrVar ComplexTypeOrVar(string _source) => Parse<ComplexTypeOrVar>(_source, _p => _p.complexTypeOrVar_Greedy());
        public static IdentifierOrDiscard IdentifierOrDiscard(string _source) => Parse<IdentifierOrDiscard>(_source, _p => _p.identifierOrDiscard_Greedy());
        public static ComplexTypeOrExpression ComplexTypeOrExpression(string _source) => Parse<ComplexTypeOrExpression>(_source, _p => _p.complexTypeOrExpression_Greedy());
        // Statements
        public static Statement Statement(string _source) => Parse<Statement>(_source, _p => _p.statement_Greedy());

    }

}
