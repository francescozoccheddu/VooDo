using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class AndOpExpr : BinaryOpExpr
    {

        internal AndOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 7;
        protected sealed override string m_OperatorSymbol => Syntax.Symbols.andOp;
    }

}
