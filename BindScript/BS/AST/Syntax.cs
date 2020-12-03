using BS.AST.Expressions;
using BS.AST.Expressions.Fundamentals;
using BS.AST.Expressions.Literals;
using BS.AST.Expressions.Operators;
using BS.AST.Expressions.Operators.Operations;
using BS.AST.Statements;

using System.Collections.Generic;

namespace BS.AST
{

    public static class Syntax
    {

        public interface ICode
        {
            string Code { get; }
        }

        public interface IExpr : ICode
        {
            int Priority { get; }
        }

        public static class Symbols
        {
            public const string sumOp = "+";
            public const string diffOp = "-";
            public const string prodOp = "*";
            public const string divOp = "/";
            public const string andOp = "&";
            public const string orOp = "|";
            public const string notOp = "!";
            public const string negOp = "-";
            public const string eqOp = "=";
            public const string neqOp = "!=";
            public const string geOp = ">=";
            public const string leOp = "<=";
            public const string gtOp = ">";
            public const string ltOp = "<";
            public const string easeOp = "by";
            public const string assignOp = ":";
            public const string assignEnd = ";";
            public const string expGroupL = "(";
            public const string expGroupR = ")";
            public const string expSubsL = "[";
            public const string expSubsR = "]";
            public const string statGroupL = "{";
            public const string statGroupR = "}";
            public const string ifStat = "if";
            public const string elseStat = "else";
            public const string whileStat = "while";
            public const string forStat = "for";
            public const string trueLit = "true";
            public const string falseLit = "false";
            public const string nullLit = "null";
            public const string stringLitDelim = "\"";
            public const string stringLitEscape = "\\";
            public const string qualNameSep = "::";
            public const string argListSep = ",";
            public const string memberOp = ".";
            public const string indent = "    ";
        }

        public static string FormatEaseExp(string _easeable, string _ease)
            => $"{_easeable} {Symbols.easeOp} {_ease}";

        public static string FormatAssignStat(string _left, string _right)
            => $"{_left} {Symbols.assignOp} {_right}{Symbols.assignEnd}";

        public static string FormatSubsExp(string _indexable, string _index)
            => $"{_indexable} {Symbols.expSubsL}{_index}{Symbols.expSubsR}";

        public static string FormatSequenceStat(IEnumerable<string> _stats)
            => $"{Symbols.statGroupL}\n{Indent(string.Join("\n", _stats))}\n{Symbols.statGroupR}";

        public static string FormatIfStat(string _cond, string _then)
            => $"{Symbols.ifStat} {WrapExp(_cond)}\n{_then}\n";

        public static string FormatIfElseStat(string _cond, string _then, string _else)
            => $"{Symbols.ifStat} {WrapExp(_cond)}\n{_then}\n{Symbols.elseStat}\n{_else}\n";

        public static string FormatWhileStat(string _cond, string _body)
            => $"{Symbols.whileStat} {WrapExp(_cond)}\n{_body}\n";

        public static string FormatForStat(string _target, string _source, string _body)
            => $"{Symbols.forStat} {WrapExp($"{_target} {Symbols.assignOp} {_source}")}\n{_body}\n";

        public static string WrapExp(string _arg)
            => $"{Symbols.expGroupL}{_arg}{Symbols.expGroupR}";

        public static string FormatQualName(IEnumerable<string> _pathAndFinal)
            => string.Join(Symbols.qualNameSep, _pathAndFinal);

        public static string FormatCallExp(string _callable, string _argList)
            => $"{_callable}{_argList}";

        public static string FormatCastExp(string _type, string _target)
            => $"{WrapExp(_type)} {_target}";

        public static string FormatMemberExp(string _target, string _member)
            => $"{_target}{Symbols.memberOp}{_member}";

        public static string FormatArgList(IEnumerable<string> _args)
            => WrapExp(string.Join(Symbols.argListSep, _args));

        public static string Indent(string _body)
            => $"{Symbols.indent}{_body.Replace("\n", $"\n{Symbols.indent}")}";

        public static string FormatBinaryOp(string _left, string _right, string _op)
            => $"{_left} {_op} {_right}";

        public static string FormatUnaryOp(string _arg, string _op)
            => $"{_op} {_arg}";

        public static string EscapeString(string _string)
            => _string
            .Replace(Symbols.stringLitEscape, Symbols.stringLitEscape + Symbols.stringLitEscape)
            .Replace(Symbols.stringLitDelim, Symbols.stringLitEscape + Symbols.stringLitDelim);

        public static string FormatLitExp(string _string)
            => $"{Symbols.stringLitDelim}{EscapeString(_string)}{Symbols.stringLitDelim}";

        public static string FormatLitExp(int _int)
            => _int.ToString();

        public static string FormatLitExp(double _real)
            => _real.ToString();

        public static string FormatLitExp(bool _bool)
            => _bool ? Symbols.trueLit : Symbols.falseLit;

    }

}
