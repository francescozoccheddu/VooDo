
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;

using System;

namespace BS.AST.Statements
{
    public sealed class ForStat : Stat
    {

        internal ForStat(Name _target, Expr _source, Stat _body)
        {
            Ensure.NonNull(_target, nameof(_target));
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_body, nameof(_body));
            TargetName = _target;
            Source = _source;
            Body = _body;
        }

        public Name TargetName { get; }
        public Expr Source { get; }
        public Stat Body { get; }

        public override string Code => Syntax.FormatForStat(TargetName, Source.Code, Body.Code);

        internal override void Run(Env _env) => throw new NotImplementedException();

    }
}
