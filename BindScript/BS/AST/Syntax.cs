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

        public const string sumOp = "+";
        public const string diffOp = "-";
        public const string prodOp = "*";
        public const string fractOp = "/";
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
        public const string forInSep = "in";
        public const string trueLit = "true";
        public const string falseLit = "false";
        public const string nullLit = "null";
        public const string qualNameSep = "::";
        public const string argListSep = ",";


        public static string FormatSumExp(string _left, string _right)
            => $"{_left} {sumOp} {_right}";

        public static string FormatDiffExp(string _left, string _right)
            => $"{_left} {diffOp} {_right}";

        public static string FormatProdExp(string _left, string _right)
            => $"{_left} {prodOp} {_right}";

        public static string FormatFractExp(string _left, string _right)
            => $"{_left} {fractOp} {_right}";

        public static string FormatAndExp(string _left, string _right)
            => $"{_left} {andOp} {_right}";

        public static string FormatOrExp(string _left, string _right)
            => $"{_left} {orOp} {_right}";

        public static string FormatNotExp(string _arg)
            => $"{notOp}{_arg}";

        public static string FormatNegExp(string _arg)
            => $"{negOp}{_arg}";

        public static string FormatGeExp(string _left, string _right)
            => $"{_left} {geOp} {_right}";

        public static string FormatLeExp(string _left, string _right)
            => $"{_left} {leOp} {_right}";

        public static string FormatGtExp(string _left, string _right)
            => $"{_left} {gtOp} {_right}";

        public static string FormatLtExp(string _left, string _right)
            => $"{_left} {ltOp} {_right}";

        public static string FormatEaseExp(string _easeable, string _ease)
            => $"{_easeable} {easeOp} {_ease}";

        public static string FormatAssignStat(string _left, string _right)
            => $"{_left} {assignOp} {_right}{assignEnd}";

        public static string FormatSubsExp(string _indexable, string _index)
            => $"{_indexable} {expSubsL}{_index}{expSubsR}";

        public static string FormatGroupStat(IEnumerable<string> _stats)
            => $"{statGroupL}\n{Indent(string.Join("\n", _stats))}\n{statGroupR}";

        public static string FormatIfStat(string _cond, string _then)
            => $"{ifStat} {FormatGroupExp(_cond)}\n{IndentIf(_then)}\n";

        public static string FormatIfElseStat(string _cond, string _then, string _else)
            => $"{ifStat} {FormatGroupExp(_cond)}\n{IndentIf(_then)}\n{elseStat}\n{IndentIf(_else)}\n";

        public static string FormatWhileStat(string _cond, string _body)
            => $"{whileStat} {FormatGroupExp(_cond)}\n{IndentIf(_body)}\n";

        public static string FormatForStat(string _target, string _source, string _body)
            => $"{forStat} {FormatGroupExp($"{_target} {forInSep} {_source}")}\n{IndentIf(_body)}\n";

        public static string FormatGroupExp(string _arg)
            => $"{expGroupL}{_arg}{expGroupR}";

        public static string FormatQualName(IEnumerable<string> _pathAndFinal)
            => string.Join(qualNameSep, _pathAndFinal);

        public static string FormatCallExp(string _callable, IEnumerable<string> _args)
            => $"{_callable}{FormatGroupExp(string.Join(argListSep, _args))}";

        private static string IndentIf(string _body)
            => _body.StartsWith(statGroupL) && _body.EndsWith(statGroupR) ? Indent(_body) : _body;

        public static string Indent(string _body)
            => "\t" + _body.Replace("\n", "\n\t");


    }

}
