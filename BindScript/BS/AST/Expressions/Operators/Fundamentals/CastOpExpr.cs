using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Operators.Fundamentals
{

    public sealed class CastExpr : UnaryOpExpr
    {

        internal CastExpr(Expr _argument, Type _targetType) : base(_argument)
        {
            Ensure.NonNull(_targetType, nameof(_targetType));
            TargetType = _targetType;
        }

        public Type TargetType { get; }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

    }

}
