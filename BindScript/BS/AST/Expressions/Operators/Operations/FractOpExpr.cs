using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class FractOpExpr : BinaryOpExpr
    {

        internal FractOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 3;
        protected sealed override string m_OperatorSymbol => Syntax.Symbols.fractOp;
    }

}
