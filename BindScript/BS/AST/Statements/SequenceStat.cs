
using BS.Exceptions;
using BS.Runtime;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST.Statements
{
    public sealed class SequenceStat : Stat
    {

        internal SequenceStat(params Stat[] _stats) : this((IEnumerable<Stat>) _stats)
        {

        }

        internal SequenceStat(IEnumerable<Stat> _stats)
        {
            Ensure.NonNull(_stats, nameof(_stats));
            Statements = new ReadOnlyCollection<Stat>(_stats.ToArray());
        }

        public IReadOnlyList<Stat> Statements { get; }

        #region Stat

        internal sealed override void Run(Env _env) => throw new NotImplementedException();

        #endregion

        #region ASTBase

        public sealed override string Code => Syntax.FormatSequenceStat(Statements.Select(_s => _s.Code));


        public sealed override bool Equals(object _obj)
            => _obj is SequenceStat stat && Statements.Equals(stat.Statements);

        public sealed override int GetHashCode()
            => Statements.GetHashCode();

        #endregion

    }
}
