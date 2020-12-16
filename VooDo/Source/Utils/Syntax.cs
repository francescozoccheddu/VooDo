using VooDo.AST.Expressions;
using VooDo.AST.Statements;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.Utils
{

    internal static class Syntax
    {

        internal static string WrappedCode(this Expr _expr) => $"({_expr.Code})";

        internal static string WrappedCodeIf(this Expr _expr, bool _condition) => _condition ? _expr.WrappedCode() : _expr.Code;

        internal static string LeftCode(this Expr _expr, int _precedence) => _expr.WrappedCodeIf(_expr.Precedence > _precedence);

        internal static string RightCode(this Expr _expr, int _precedence) => _expr.WrappedCodeIf(_expr.Precedence >= _precedence);

        internal static string ArgumentsListCode(this IEnumerable<Expr> _arguments) => string.Join(", ", _arguments.Select(_a => _a.Code));

        internal static string IndentedCode(this Stat _stat) => _stat is SequenceStat ? _stat.Code : ("\t" + _stat.Code.Replace("\n", "\n\t"));

        internal static string EscapeString(string _string)
        {
            StringBuilder literal = new StringBuilder(_string.Length + 2);
            literal.Append("\"");
            foreach (var c in _string)
            {
                switch (c)
                {
                    case '\'':
                    literal.Append(@"\'");
                    break;
                    case '\"':
                    literal.Append("\\\"");
                    break;
                    case '\\':
                    literal.Append(@"\\");
                    break;
                    case '\0':
                    literal.Append(@"\0");
                    break;
                    case '\a':
                    literal.Append(@"\a");
                    break;
                    case '\b':
                    literal.Append(@"\b");
                    break;
                    case '\f':
                    literal.Append(@"\f");
                    break;
                    case '\n':
                    literal.Append(@"\n");
                    break;
                    case '\r':
                    literal.Append(@"\r");
                    break;
                    case '\t':
                    literal.Append(@"\t");
                    break;
                    case '\v':
                    literal.Append(@"\v");
                    break;
                    default:
                    if (c >= 0x20 && c <= 0x7e)
                    {
                        literal.Append(c);
                    }
                    else
                    {
                        literal.Append(@"\u");
                        literal.Append(((int) c).ToString("x4"));
                    }
                    break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }

        internal static string UnescapeString(string _string)
        {
            return _string;
        }

    }

}
