
using BS.AST.Expressions;
using BS.Exceptions;
using BS.Runtime;

using System;

namespace BS.AST.Statements
{
    public sealed class AssignmentStat : Stat
    {

        internal AssignmentStat(Expr _target, Expr _source)
        {
            Ensure.NonNull(_target, nameof(_target));
            Ensure.NonNull(_source, nameof(_source));
            Target = _target;
            Source = _source;
        }

        public Expr Target { get; }
        public Expr Source { get; }

        public sealed override string Code => Syntax.FormatAssignStat(Target.Code, Source.Code);

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

    }
}
