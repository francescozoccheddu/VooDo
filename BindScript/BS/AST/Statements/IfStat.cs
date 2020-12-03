
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;
using BS.Utils;

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

        #region Stat

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

        #endregion

        #region ASTBase

        public sealed override string Code
        {
            get
            {
                string thenBody = ThenBody is SequenceStat ? ThenBody.Code : Syntax.Indent(ThenBody.Code);
                if (HasElse)
                {
                    string elseBody = ElseBody is SequenceStat ? ElseBody.Code : Syntax.Indent(ElseBody.Code);
                    return Syntax.FormatIfElseStat(Condition.Code, thenBody, elseBody);
                }
                else
                {
                    return Syntax.FormatIfStat(Condition.Code, thenBody);
                }
            }
        }

        public sealed override bool Equals(object _obj)
            => _obj is IfStat stat && Condition.Equals(Condition) && ThenBody.Equals(stat.ThenBody) && Identity.AreEqual(ElseBody, stat.ElseBody);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Condition, ThenBody, ElseBody);

        #endregion

    }
}
