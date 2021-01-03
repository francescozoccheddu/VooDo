
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Operators
{

    public abstract class UnaryOpExpr : Expr
    {

        internal UnaryOpExpr(Expr _argument)
        {
            Ensure.NonNull(_argument, nameof(_argument));
            Argument = _argument;
        }

        public Expr Argument { get; }

        protected abstract string m_OperatorSymbol { get; }

        protected abstract object Evaluate(dynamic _argument);

        #region ASTBase

        public sealed override string Code
            => $"{m_OperatorSymbol}{Argument.LeftCode(Precedence)}";

        public sealed override bool Equals(object _obj)
            => _obj is UnaryOpExpr expr && Argument.Equals(expr.Argument);

        public sealed override int GetHashCode() => Argument.GetHashCode();

        internal sealed override object Evaluate(Env _env) => Evaluate(Argument.Evaluate(_env));

        #endregion

    }

}
