
using VooDo.AST.Expressions;
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

        internal sealed override void Run(Runtime.Env _env)
        {
            while (Reflection.Cast<bool>(Condition.Evaluate(_env)))
            {
                Body.Run(_env);
            }
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
