
using VooDo.AST.Expressions;
using VooDo.Runtime;
using VooDo.Utils;

using System;

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

        internal sealed override void Run(Runtime.Env _env) => throw new NotImplementedException();

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
