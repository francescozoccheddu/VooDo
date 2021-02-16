using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.WinUI.Interfaces
{

    public sealed class TargetProviderList : IReadOnlyList<ITargetProvider>, ITargetProvider
    {

        private readonly ImmutableArray<ITargetProvider> m_providers;

        public TargetProviderList(IEnumerable<ITargetProvider> _providers)
        {
            m_providers = _providers.ToImmutableArray();
        }

        public ITargetProvider this[int _index] => m_providers[_index];

        public int Count => m_providers.Length;

        public IEnumerator<ITargetProvider> GetEnumerator() => ((IEnumerable<ITargetProvider>) m_providers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_providers).GetEnumerator();

        public Target? GetTarget(object _targetPrototype)
            => m_providers.Select(_p => _p.GetTarget(_targetPrototype)).First(_t => _t is not null);

    }

}
