using System;
using System.Collections.Generic;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class IsExpr : Expr
    {

        internal IsExpr(Expr _source, Expr _testType)
        {
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_testType, nameof(_testType));
            Source = _source;
            TestType = _testType;
        }

        public Expr Source { get; }
        public Expr TestType { get; }

        #region Expr

        public sealed override int Precedence => 5;

        public sealed override string Code =>
            $"{Source.LeftCode(Precedence)} is {TestType.RightCode(Precedence)}";

        internal sealed override object Evaluate(Runtime.Env _env)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is CastExpr expr && Source.Equals(expr.Source) && TestType.Equals(expr.TargetType);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Source, TestType);

        #endregion

    }

}
