using System;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class VarExpr : Expr
    {

        internal VarExpr(Name _name) : this(new QualifiedName(_name))
        {
        }

        internal VarExpr(QualifiedName _name)
        {
            Ensure.NonNull(_name, nameof(_name));
            Name = _name;
        }

        public QualifiedName Name { get; }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        internal sealed override void Assign(Env _env, object _value)
        {
            throw new NotImplementedException();
        }

        public sealed override int Precedence => 0;

        public sealed override string Code => Name.Code;

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is VarExpr expr && Name.Equals(expr.Name);

        public sealed override int GetHashCode()
            => Name.GetHashCode();

        #endregion

    }

}
