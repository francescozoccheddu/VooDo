using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Comparisons
{

    public sealed class GeOpExpr : BinaryOpExpr
    {

        internal GeOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

    }

}
