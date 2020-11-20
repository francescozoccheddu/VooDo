using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class NotOpExpr : UnaryOpExpr
    {

        internal NotOpExpr(Expr _argument) : base(_argument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 1;
        protected sealed override string m_OperatorSymbol => Syntax.Symbols.notOp;
    }

}
