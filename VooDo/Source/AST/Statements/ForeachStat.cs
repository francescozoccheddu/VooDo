﻿
using VooDo.AST.Expressions;
using VooDo.Runtime;
using VooDo.Utils;

using System;

namespace VooDo.AST.Statements
{
    public sealed class ForeachStat : Stat
    {

        internal ForeachStat(Expr _target, Expr _source, Stat _body)
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

        #region Stat

        internal sealed override void Run(Runtime.Env _env) => throw new NotImplementedException();

        #endregion

        #region ASTBase

        public sealed override string Code
            => $"foreach ({Target.Code} in {Source.Code})\n{Body.IndentedCode()}";

        public sealed override bool Equals(object _obj)
            => _obj is WhileStat stat && Target.Equals(Target) && Source.Equals(Source) && Body.Equals(stat.Body);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Target, Source, Body);

        #endregion
    }
}
