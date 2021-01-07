using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.AST.Expressions.Fundamentals;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.Source.Utils
{

    internal static class Expressions
    {

        internal static T As<T>(this Expr _expr, Env _env) => Reflection.Cast<T>(_expr.Evaluate(_env).Value);

        internal static Type AsType(this Expr _expr, Env _env) => _expr.As<Type>(_env);

        internal static void Evaluate(this IReadOnlyList<Expr> _expr, Env _env, out object[] _values, out Type[] _types)
        {
            _values = new object[_expr.Count];
            _types = new Type[_expr.Count];
            for (int i = 0; i < _expr.Count; i++)
            {
                _values[i] = _expr[i].Evaluate(_env, out _types[i]);
            }
        }

        internal static object Evaluate(this Expr _expr, Env _env, out Type _type)
        {
            if (_expr is CastExpr cast)
            {
                return cast.Evaluate(_env, out _type);
            }
            else
            {
                object value = _expr.Evaluate(_env);
                _type = value.GetType();
                return value;
            }
        }

        internal static bool AsBool(this Expr _expr, Env _env) => _expr.As<bool>(_env);

    }

}
