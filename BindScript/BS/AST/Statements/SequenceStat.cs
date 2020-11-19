
using BS.Exceptions;
using BS.Runtime;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST.Statements
{
    public sealed class SequenceStat : Stat, IReadOnlyList<Stat>
    {

        internal SequenceStat(IEnumerable<Stat> _stats)
        {
            Ensure.NonNull(_stats, nameof(_stats));
            m_stats = _stats.ToArray();
        }

        private readonly Stat[] m_stats;

        public int Count => m_stats.Length;

        public override string Code => Syntax.FormatGroupStat(m_stats.Select(_s => _s.Code));

        public Stat this[int _index] => m_stats[_index];

        public IEnumerator<Stat> GetEnumerator() => ((IEnumerable<Stat>) m_stats).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_stats.GetEnumerator();

        internal override void Run(Env _env) => throw new NotImplementedException();

    }
}
