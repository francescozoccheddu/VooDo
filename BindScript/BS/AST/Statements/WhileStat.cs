
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;
using BS.Utils;

using System;
using System.Collections.Generic;

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

        #region Stat

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

        #endregion

        #region ASTBase

        public sealed override string Code
            => Syntax.FormatWhileStat(Condition.Code, Body is SequenceStat ? Body.Code : Syntax.Indent(Body.Code));

        public sealed override bool Equals(object _obj)
            => _obj is WhileStat stat && Condition.Equals(Condition) && Body.Equals(stat.Body);

        public sealed override int GetHashCode()
            => Hash.Combine(Condition, Body);

        #endregion

    }
}
