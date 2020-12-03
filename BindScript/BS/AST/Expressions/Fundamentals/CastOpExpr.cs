using System;
using System.Collections.Generic;

using BS.Exceptions;
using BS.Runtime;
using BS.Utils;

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

        #region Expr

        public sealed override int Priority => 2;

        public sealed override string Code => Syntax.FormatCastExp(TargetType.Code, Target.Priority > Priority ? Syntax.WrapExp(Target.Code) : Target.Code);

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is CastExpr expr && Target.Equals(expr.Target) && TargetType.Equals(expr.TargetType);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Target, TargetType);

        #endregion

    }

}
