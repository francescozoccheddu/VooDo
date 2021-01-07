
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Statements
{
    public sealed class IfElseStat : Stat
    {

        internal IfElseStat(Expr _condition, Stat _then, Stat _else)
        {
            Ensure.NonNull(_condition, nameof(_condition));
            Ensure.NonNull(_then, nameof(_then));
            Condition = _condition;
            ThenBody = _then;
            ElseBody = _else;
        }

        public Expr Condition { get; }
        public Stat ThenBody { get; }
        public Stat ElseBody { get; }
        public bool HasElse => ElseBody != null;

        #region Stat

        internal sealed override void Run(Runtime.Env _env)
        {
            bool condValue = Condition.AsBool(_env);
            (condValue ? ThenBody : ElseBody).Run(_env);
            (condValue ? ElseBody : ThenBody).Unsubscribe(_env.Script.HookManager);
        }

        public override void Unsubscribe(HookManager _hookManager)
        {
            Condition.Unsubscribe(_hookManager);
            ThenBody.Unsubscribe(_hookManager);
            ElseBody.Unsubscribe(_hookManager);
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Condition, ThenBody, ElseBody).ToHashSet();

        #endregion

        #region ASTBase

        public sealed override string Code
            => $"if ({Condition.Code})\n{ThenBody.IndentedCode()}" + (HasElse ? $"\nelse\n{ElseBody.IndentedCode()}" : "");

        public sealed override bool Equals(object _obj)
            => _obj is IfElseStat stat && Condition.Equals(Condition) && ThenBody.Equals(stat.ThenBody) && Identity.AreEqual(ElseBody, stat.ElseBody);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Condition, ThenBody, ElseBody);

        #endregion

    }
}
