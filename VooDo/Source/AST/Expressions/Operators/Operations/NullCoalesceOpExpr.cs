
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Operators
{

    public sealed class NullCoalesceOpExpr : Expr
    {

        internal NullCoalesceOpExpr(Expr _source, Expr _else)
        {
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_else, nameof(_else));
            Source = _source;
            Else = _else;
        }

        public Expr Source { get; }
        public Expr Else { get; }

        #region Expr

        public override void Unsubscribe(HookManager _hookManager)
        {
            Source.Unsubscribe(_hookManager);
            Else.Unsubscribe(_hookManager);
        }

        public sealed override int Precedence => 12;

        public override string Code => $"{Source.Code} ?? {Else.Code}";

        #endregion

        internal override Eval Evaluate(Env _env)
        {
            Eval source = Source.Evaluate(_env);
            if (source.Value != null)
            {
                Else.Unsubscribe(_env.Script.HookManager);
                return source;
            }
            else
            {
                return Else.Evaluate(_env);
            }
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Source, Else).ToHashSet();

        public override bool Equals(object _obj)
            => _obj is NullCoalesceOpExpr expr && Source.Equals(expr.Source) && Else.Equals(expr.Else);

        public override int GetHashCode()
            => Identity.CombineHash(Source, Else);

    }

}
