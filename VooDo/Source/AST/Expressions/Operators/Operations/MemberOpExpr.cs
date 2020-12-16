using System;

using VooDo.Exceptions;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Operators
{

    public sealed class MemberOpExpr : BinaryOpExpr
    {

        internal MemberOpExpr(Expr _source, Expr _member, bool _nullCoalesce) : base(_source, _member)
        {
            NullCoalesce = _nullCoalesce;
        }

        public Expr Source => LeftArgument;
        public Expr Member => RightArgument;
        public bool NullCoalesce { get; }

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

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => NullCoalesce ? "?." : ".";

        protected sealed override bool m_SpaceBetweenOperator => false;

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is MemberOpExpr expr && NullCoalesce == expr.NullCoalesce && base.Equals(expr);

        public sealed override int GetHashCode()
            => Identity.CombineHash(base.GetHashCode(), NullCoalesce);

        #endregion

    }

}
