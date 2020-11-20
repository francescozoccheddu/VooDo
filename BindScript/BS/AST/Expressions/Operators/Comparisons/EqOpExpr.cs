using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Comparisons
{

    public sealed class EqOpExpr : BinaryOpExpr
    {

        internal EqOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 6;

        protected sealed override string m_OperatorSymbol => Syntax.Symbols.eqOp;
    }

}
