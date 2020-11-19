using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Operators.Fundamentals
{

    public sealed class CallOpExpr : UnaryOpExpr
    {

        internal CallOpExpr(Expr _callable, ArgList _argumentList) : base(_callable)
        {
            Ensure.NonNull(_argumentList, nameof(_argumentList));
            ArgumentList = _argumentList;
        }

        public ArgList ArgumentList { get; }
        public Expr Callable => Argument;

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

    }

}
