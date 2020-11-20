using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions.Fundamentals
{

    public sealed class MemberExpr : Expr
    {

        internal MemberExpr(Expr _target, Name _name)
        {
            Ensure.NonNull(_target, nameof(_target));
            Ensure.NonNull(_name, nameof(_name));
            Target = _target;
            Name = _name;
        }

        public Expr Target { get; }
        public Name Name { get; }

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
            => Syntax.FormatMemberExp(Target.Priority > Priority ? Syntax.WrapExp(Target.Code) : Target.Code, Name.Code);
    }

}
