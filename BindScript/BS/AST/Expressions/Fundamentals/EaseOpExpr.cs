using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class EaseOpExpr : Expr
    {

        internal EaseOpExpr(Expr _easeable, Expr _ease)
        {
            Ensure.NonNull(_easeable, nameof(_easeable));
            Ensure.NonNull(_ease, nameof(_ease));
            Easeable = _easeable;
            Ease = _ease;
        }

        public Expr Easeable { get; }
        public Expr Ease { get; }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 9;

        public sealed override string Code
            => Syntax.FormatEaseExp
            (Easeable.Priority > Priority ? Syntax.WrapExp(Easeable.Code) : Easeable.Code,
                Ease.Priority >= Priority ? Syntax.WrapExp(Ease.Code) : Ease.Code);
    }

}
