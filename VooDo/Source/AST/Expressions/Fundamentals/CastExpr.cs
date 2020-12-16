using System;
using System.Collections.Generic;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CastExpr : Expr
    {

        internal CastExpr(Expr _targetType, Expr _source)
        {
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_targetType, nameof(_targetType));
            Source = _source;
            TargetType = _targetType;
        }

        public Expr Source { get; }
        public Expr TargetType { get; }

        #region Expr

        public sealed override int Priority => 2;

        public sealed override string Code =>
            $"({TargetType.Code}) {Source.RightCode(Priority)}";

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is CastExpr expr && Source.Equals(expr.Source) && TargetType.Equals(expr.TargetType);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Source, TargetType);

        #endregion

    }

}
