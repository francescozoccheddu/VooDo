using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class CallOpExpr : Expr
    {

        internal CallOpExpr(Expr _callable, ArgList _argumentList)
        {
            Ensure.NonNull(_callable, nameof(_callable));
            Ensure.NonNull(_argumentList, nameof(_argumentList));
            Callable = _callable;
            ArgumentList = _argumentList;
        }

        public Expr Callable { get; }
        public ArgList ArgumentList { get; }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 0;

        public sealed override string Code => Syntax.FormatCallExp(Callable.Priority > Priority ? Syntax.WrapExp(Callable.Code) : Callable.Code, ArgumentList.Code);
    }

}
