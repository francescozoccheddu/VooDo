
using System.Collections.Generic;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class MemberExpr : Expr
    {

        internal MemberExpr(Expr _source, Name _member, bool _nullCoalesce)
        {
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_member, nameof(_member));
            Source = _source;
            Member = _member;
            NullCoalesce = _nullCoalesce;
        }

        public Expr Source { get; }
        public Name Member { get; }
        public bool NullCoalesce { get; }

        #region Expr

        internal sealed override Eval Evaluate(Env _env)
        {
            Eval source = Source.Evaluate(_env);
            if (source.Value == null && NullCoalesce)
            {
                return new Eval(null);
            }
            Eval eval = Reflection.EvaluateMember(_env, source, Member);
            _env.Script.HookManager.Subscribe(this, source, Member);
            return eval;
        }

        internal sealed override void Assign(Env _env, Eval _value)
        {
            Eval source = Source.Evaluate(_env);
            Reflection.AssignMember(_env, source, Member, _value);
            _env.Script.HookManager.Subscribe(this, source, Member);
        }

        internal override HashSet<Name> GetVariables() => Source.GetVariables();

        public override void Unsubscribe(HookManager _hookManager)
        {
            _hookManager.Unsubscribe(this);
            Source.Unsubscribe(_hookManager);
        }

        public sealed override int Precedence => 0;

        public override string Code => $"{Source.LeftCode(Precedence)}{(NullCoalesce ? "?." : ".")}{Member}";

        #endregion


        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is MemberExpr expr && NullCoalesce == expr.NullCoalesce && Source.Equals(expr.Source) && Member.Equals(expr.Member);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Source, Member, NullCoalesce);

        #endregion

    }

}
