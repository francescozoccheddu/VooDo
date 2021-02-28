using Antlr4.Runtime;

using System;
using System.IO;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Parsing.Generated;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Parsing
{

    public static class Parser
    {

        private sealed class ErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
        {

            private readonly string m_source;
            private readonly string? m_sourcePath;

            internal ErrorListener(string _source, string? _sourcePath)
            {
                m_source = _source;
                m_sourcePath = _sourcePath;
            }

            private void Throw(string _message, IRecognizer _recognizer, RuleContext? _rule, IToken? _token)
            {
                int? startingChar = (_recognizer as VooDoLexer)?.TokenStartCharIndex;
                _rule ??= (_recognizer as VooDoParser)?.RuleContext;
                _token ??= (_recognizer as VooDoLexer)?.Token;
                _token ??= (_recognizer as VooDoParser)?.CurrentToken;
                Throw(_message, startingChar, _rule, _token);
            }

            private void Throw(string _message, int? _startingChar, RuleContext? _rule, IToken? _token)
            {
                int start, end;
                if (_token is not null)
                {
                    start = _token.StartIndex;
                    end = _token.StopIndex;
                }
                else if (_rule is ParserRuleContext rule)
                {
                    start = rule.Start.StartIndex;
                    end = (rule.Stop ?? rule.Start).StopIndex;
                }
                else if (_startingChar is not null)
                {
                    start = _startingChar.Value;
                    end = start + 1;
                }
                else
                {
                    start = end = 0;
                }
                end = Math.Max(end, start);
                CodeOrigin? origin = new CodeOrigin(start, end - start, m_source, m_sourcePath);
                throw new ParsingError(_message, origin).AsThrowable();
            }

            public void SyntaxError(TextWriter _output, IRecognizer _recognizer, int _offendingSymbol, int _line, int _charPositionInLine, string _msg, RecognitionException _e)
                => Throw(_msg, _recognizer, _e.Context, null);


            public void SyntaxError(TextWriter _output, IRecognizer _recognizer, IToken _offendingSymbol, int _line, int _charPositionInLine, string _msg, RecognitionException _e)
                => Throw(_msg, _recognizer, _e.Context, _offendingSymbol);

        }

        private static VooDoParser MakeParser(string _script, string? _sourcePath)
        {
            ErrorListener listener = new ErrorListener(_script, _sourcePath);
            VooDoLexer lexer = new(new AntlrInputStream(_script));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(listener);
            VooDoParser parser = new(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(listener);
            return parser;
        }

        private static TNode Parse<TNode>(string _source, string? _sourcePath, Func<VooDoParser, ParserRuleContext> _ruleProvider) where TNode : Node
            => (TNode)new Visitor(_source, _sourcePath).Visit(_ruleProvider(MakeParser(_source, _sourcePath))).SetAsRoot();

        // Script
        public static Script Script(string _source, string? _sourcePath = null) => Parse<Script>(_source, _sourcePath, _p => _p.script_Greedy());
        public static Script ScriptOrExpression(string _source, string? _sourcePath = null) => Parse<Script>(_source, _sourcePath, _p => _p.scriptOrExpression_Greedy());
        // Directives
        public static UsingDirective UsingDirective(string _source, string? _sourcePath = null) => Parse<UsingDirective>(_source, _sourcePath, _p => _p.usingDirective_Greedy());
        // Expressions
        public static Expression Expression(string _source, string? _sourcePath = null) => Parse<Expression>(_source, _sourcePath, _p => _p.expression_Greedy());
        // Names
        public static ComplexType ComplexType(string _source, string? _sourcePath = null) => Parse<ComplexType>(_source, _sourcePath, _p => _p.complexType_Greedy());
        public static SimpleType SimpleType(string _source, string? _sourcePath = null) => Parse<SimpleType>(_source, _sourcePath, _p => _p.simpleType_Greedy());
        public static QualifiedType QualifiedType(string _source, string? _sourcePath = null) => Parse<QualifiedType>(_source, _sourcePath, _p => _p.qualifiedType_Greedy());
        public static TupleType TupleType(string _source, string? _sourcePath = null) => Parse<TupleType>(_source, _sourcePath, _p => _p.tupleType_Greedy());
        public static Identifier Identifier(string _source, string? _sourcePath = null) => Parse<Identifier>(_source, _sourcePath, _p => _p.identifier_Greedy());
        public static Namespace Namespace(string _source, string? _sourcePath = null) => Parse<Namespace>(_source, _sourcePath, _p => _p.namespace_Greedy());
        public static ComplexTypeOrVar ComplexTypeOrVar(string _source, string? _sourcePath = null) => Parse<ComplexTypeOrVar>(_source, _sourcePath, _p => _p.complexTypeOrVar_Greedy());
        public static IdentifierOrDiscard IdentifierOrDiscard(string _source, string? _sourcePath = null) => Parse<IdentifierOrDiscard>(_source, _sourcePath, _p => _p.identifierOrDiscard_Greedy());
        public static ComplexTypeOrExpression ComplexTypeOrExpression(string _source, string? _sourcePath = null) => Parse<ComplexTypeOrExpression>(_source, _sourcePath, _p => _p.complexTypeOrExpression_Greedy());
        // Statements
        public static Statement Statement(string _source, string? _sourcePath = null) => Parse<Statement>(_source, _sourcePath, _p => _p.statement_Greedy());

    }

}
