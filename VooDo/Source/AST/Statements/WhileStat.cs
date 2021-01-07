
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Statements
{
    public sealed class WhileStat : Stat
    {

        internal WhileStat(Expr _condition, Stat _body)
        {
            Ensure.NonNull(_condition, nameof(_condition));
            Ensure.NonNull(_body, nameof(_body));
            Condition = _condition;
            Body = _body;
        }

        public Expr Condition { get; }
        public Stat Body { get; }

        #region Stat

        internal sealed override void Run(Env _env)
        {
            bool done = false;
            while (Condition.AsBool(_env))
            {
                done = true;
                Body.Run(_env);
            }
            if (!done)
            {
                Body.Unsubscribe(_env.Script.HookManager);
            }
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Condition, Body).ToHashSet();

        public override void Unsubscribe(HookManager _hookManager)
        {
            Condition.Unsubscribe(_hookManager);
            Body.Unsubscribe(_hookManager);
        }

        #endregion

        #region ASTBase

        public sealed override string Code
            => $"while ({Condition.Code})\n{Body.IndentedCode()}";

        public sealed override bool Equals(object _obj)
            => _obj is WhileStat stat && Condition.Equals(Condition) && Body.Equals(stat.Body);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Condition, Body);

        #endregion

    }
}
