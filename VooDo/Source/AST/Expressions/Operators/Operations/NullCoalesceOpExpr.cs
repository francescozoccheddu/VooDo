using System;

namespace VooDo.AST.Expressions.Operators
{

    public sealed class NullCoalesceOpExpr : BinaryOpExpr
    {

        internal NullCoalesceOpExpr(Expr _source, Expr _member) : base(_source, _member)
        {
        }

        public Expr Source => LeftArgument;
        public Expr Else => RightArgument;

        #region Expr

        protected override object Evaluate(dynamic _left, dynamic _right) => _left ?? _right;

        internal sealed override void Assign(Runtime.Env _env, object _value) => throw new NotImplementedException();

        public sealed override int Precedence => 12;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "??";

        #endregion

    }

}
