using System;

using BS.Exceptions;
using BS.Runtime;

namespace BS.AST.Expressions
{

    public sealed class VarExpr : Expr
    {

        internal VarExpr(QualifiedName _name)
        {
            Ensure.NonNull(_name, nameof(_name));
            Name = _name;
        }

        public QualifiedName Name { get; }

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
