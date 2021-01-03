using System;
using System.Collections.Generic;

using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CallExpr : ParametricExpr
    {

        internal CallExpr(Expr _callable, IEnumerable<Expr> _arguments) : base(_callable, _arguments)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env) => throw new NotImplementedException();

        public sealed override int Precedence => 0;

        public sealed override string Code =>
               $"{Source.LeftCode(Precedence)}({Arguments.ArgumentsListCode()})";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj) => _obj is CallExpr && base.Equals(_obj);

        public override int GetHashCode() => base.GetHashCode();

        #endregion

    }

}
