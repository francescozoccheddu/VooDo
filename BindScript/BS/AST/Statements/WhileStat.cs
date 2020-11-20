
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;

using System;

namespace BS.AST.Statements
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

        public sealed override string Code
            => Syntax.FormatWhileStat(Condition.Code, Body is SequenceStat ? Body.Code : Syntax.Indent(Body.Code));

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

    }
}
