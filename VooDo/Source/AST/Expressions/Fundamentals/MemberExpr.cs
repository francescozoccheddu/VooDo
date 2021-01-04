using System;

using VooDo.Runtime.Engine;
using VooDo.Source.Utils;
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

        internal sealed override object Evaluate(Runtime.Env _env)
        {
            object sourceValue = Source.Evaluate(_env);
            if (sourceValue == null)
            {
                throw new NullReferenceException();
            }
            Type sourceType = Source is CastExpr expr ? expr.TargetType.AsType(_env) : sourceValue.GetType();
            return RuntimeHelpers.EvaluateMember(_env.Program.HookManager, Member, sourceType, sourceValue);
        }

        internal sealed override void Assign(Runtime.Env _env, object _value)
        {
            object sourceValue = Source.Evaluate(_env);
            if (sourceValue == null)
            {
                throw new NullReferenceException();
            }
            RuntimeHelpers.AssignMember(Member, _value, sourceValue.GetType(), sourceValue);
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
