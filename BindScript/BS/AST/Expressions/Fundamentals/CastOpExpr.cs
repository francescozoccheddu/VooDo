using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class CastExpr : Expr
    {

        internal CastExpr(Expr _target, Expr _targetType)
        {
            Ensure.NonNull(_target, nameof(_target));
            Ensure.NonNull(_targetType, nameof(_targetType));
            Target = _target;
            TargetType = _targetType;
        }

        public Expr Target { get; }
        public Expr TargetType { get; }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 2;

        public sealed override string Code => Syntax.FormatCastExp(TargetType.Code, Target.Priority > Priority ? Syntax.WrapExp(Target.Code) : Target.Code);
    }

}
