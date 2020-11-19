using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Operators.Fundamentals
{

    public sealed class MemberExpr : UnaryOpExpr
    {

        internal MemberExpr(Expr _argument, Name _name) : base(_argument)
        {
            Ensure.NonNull(_name, nameof(_name));
            Name = _name;
        }

        public Name Name { get; }

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
