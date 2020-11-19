
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;

using System;

namespace BS.AST.Statements
{
    public sealed class IfStat : Stat
    {

        internal IfStat(Expr _condition, Stat _then, Stat _else)
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

        public override string Code
            => HasElse ? Syntax.FormatIfElseStat(Condition.Code, ThenBody.Code, ElseBody.Code) : Syntax.FormatIfStat(Condition.Code, ThenBody.Code);

        internal override void Run(Env _env) => throw new NotImplementedException();

    }
}
