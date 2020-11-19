using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Fundamentals
{

    public sealed class EaseOpExpr : BinaryOpExpr
    {

        internal EaseOpExpr(Expr _easeable, Expr _easer) : base(_easeable, _easer)
        {
        }

        public Expr Easeable => LeftArgument;
        public Expr Easer => RightArgument;

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

    }

}
