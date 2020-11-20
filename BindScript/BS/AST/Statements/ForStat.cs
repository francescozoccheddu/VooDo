
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;

using System;

namespace BS.AST.Statements
{
    public sealed class ForStat : Stat
    {

        internal ForStat(Expr _target, Expr _source, Stat _body)
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

        public sealed override string Code
            => Syntax.FormatForStat(Target.Code, Source.Code, Body is SequenceStat ? Body.Code : Syntax.Indent(Body.Code));

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

    }
}
