using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Fundamentals
{

    public sealed class SubscriptExpr : BinaryOpExpr
    {

        internal SubscriptExpr(Expr _indexable, Expr _index) : base(_indexable, _index)
        {
        }

        public Expr Indexable => LeftArgument;
        public Expr Index => RightArgument;

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        internal sealed override void Assign(Env _env, object _value)
        {
            throw new NotImplementedException();
        }

    }

}
