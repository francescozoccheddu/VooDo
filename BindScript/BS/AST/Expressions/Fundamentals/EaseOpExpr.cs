using System;
using System.Collections.Generic;

using BS.Exceptions;
using BS.Runtime;
using BS.Utils;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class EaseOpExpr : Expr
    {

        internal EaseOpExpr(Expr _easeable, Expr _easer)
        {
            Ensure.NonNull(_easeable, nameof(_easeable));
            Ensure.NonNull(_easer, nameof(_easer));
            Easeable = _easeable;
            Easer = _easer;
        }

        public Expr Easeable { get; }
        public Expr Easer { get; }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 9;

        public sealed override string Code
            => Syntax.FormatEaseExp
            (Easeable.Priority > Priority ? Syntax.WrapExp(Easeable.Code) : Easeable.Code,
                Easer.Priority >= Priority ? Syntax.WrapExp(Easer.Code) : Easer.Code);

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is EaseOpExpr expr && Easeable.Equals(expr.Easeable) && Easer.Equals(expr.Easer);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Easeable, Easer);

        #endregion

    }

}
