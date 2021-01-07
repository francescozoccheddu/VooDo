
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Statements
{
    public sealed class ForeachStat : Stat
    {

        internal ForeachStat(Expr _target, Expr _source, Stat _body)
        {
            Ensure.NonNull(_target, nameof(_target));
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_body, nameof(_body));
            Target = _target;
            Source = _source;
            Body = _body;
        }

        public Expr Target { get; }
        public Expr Source { get; }
        public Stat Body { get; }

        #region Stat

        internal sealed override void Run(Runtime.Env _env)
        {
            bool done = false;
            foreach (Eval item in Source.As<IEnumerable>(_env))
            {
                done = true;
                Target.Assign(_env, item);
                Body.Run(_env);
            }
            if (!done)
            {
                Body.Unsubscribe(_env.Script.HookManager);
            }
        }

        public override void Unsubscribe(HookManager _hookManager)
        {
            Source.Unsubscribe(_hookManager);
            Body.Unsubscribe(_hookManager);
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Target, Source, Body).ToHashSet();

        #endregion

        #region ASTBase

        public sealed override string Code
            => $"foreach ({Target.Code} in {Source.Code})\n{Body.IndentedCode()}";

        public sealed override bool Equals(object _obj)
            => _obj is WhileStat stat && Target.Equals(Target) && Source.Equals(Source) && Body.Equals(stat.Body);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Target, Source, Body);

        #endregion
    }
}
