using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class SubscriptExpr : Expr
    {

        internal SubscriptExpr(Expr _indexable, Expr _index)
        {
            Ensure.NonNull(_indexable, nameof(_indexable));
            Ensure.NonNull(_index, nameof(_index));
            Indexable = _indexable;
            Index = _index;
        }

        public Expr Indexable { get; }
        public Expr Index { get; }

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        internal sealed override void Assign(Env _env, object _value)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 0;

        public sealed override string Code
            => Syntax.FormatSubsExp(Indexable.Priority > Priority ? Syntax.WrapExp(Indexable.Code) : Indexable.Code, Index.Code);
    }

}
