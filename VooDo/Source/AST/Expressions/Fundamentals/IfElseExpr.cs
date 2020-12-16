using System;
using System.Collections.Generic;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class IfElseExpr : Expr
    {

        internal IfElseExpr(Expr _condition, Expr _then, Expr _else)
        {
            Ensure.NonNull(_condition, nameof(_condition));
            Ensure.NonNull(_then, nameof(_then));
            Ensure.NonNull(_else, nameof(_else));
            Condition = _condition;
            Then = _then;
            Else = _else;
        }

        public Expr Condition { get; }
        public Expr Then { get; }
        public Expr Else { get; }

        #region Expr

        public sealed override int Priority => 2;

        public sealed override string Code =>
            $"{Condition.LeftCode(Priority)} ? {Then.Code} : {Else.RightCode(Priority)}";

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is IfElseExpr expr && Condition.Equals(expr.Condition) && Then.Equals(expr.Then) && Else.Equals(expr.Else);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Condition, Then, Else);

        #endregion

    }

}
