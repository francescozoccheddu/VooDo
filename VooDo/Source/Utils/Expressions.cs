using System;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Expressions.Fundamentals;
using VooDo.Runtime;
using VooDo.Runtime.Engine;

namespace VooDo.Source.Utils
{

    internal static class Expressions
    {

        internal static T As<T>(this Expr _expr, Env _env) => RuntimeHelpers.Cast<T>(_expr.Evaluate(_env));

        internal static Type AsType(this Expr _expr, Env _env) => _expr.As<Type>(_env);

        internal static Type[] ArgumentTypes(this ParametricExpr _expr, Env _env)
            => _expr.Arguments.Select(_a => _a is CastExpr expr ? RuntimeHelpers.Cast<Type>(expr.TargetType.Evaluate(_env)) : _a.GetType()).ToArray();

        internal static bool AsBool(this Expr _expr, Env _env) => _expr.As<bool>(_env);

    }

}
