
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Source.Utils;
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

        public sealed override int Precedence => 13;

        public sealed override string Code =>
            $"{Condition.LeftCode(Precedence)} ? {Then.Code} : {Else.RightCode(Precedence)}";

        internal sealed override Eval Evaluate(Env _env)
        {
            bool condition = Condition.AsBool(_env);
            (condition ? Else : Then).Unsubscribe(_env.Script.HookManager);
            return (condition ? Then : Else).Evaluate(_env);
        }

        internal override HashSet<Name> GetVariables()
         => Tree.GetVariables(Condition, Then, Else).ToHashSet();

        public override void Unsubscribe(HookManager _hookManager)
        {
            Condition.Unsubscribe(_hookManager);
            Then.Unsubscribe(_hookManager);
            Else.Unsubscribe(_hookManager);
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
