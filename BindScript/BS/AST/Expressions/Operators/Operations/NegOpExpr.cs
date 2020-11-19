using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class NegOpExpr : UnaryOpExpr
    {

        internal NegOpExpr(Expr _argument) : base(_argument)
        {
        }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

    }

}
