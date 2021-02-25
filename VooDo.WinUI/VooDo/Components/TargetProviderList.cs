using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.WinUI.Core;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class TargetProviderList : IReadOnlyList<ITargetProvider>, ITargetProvider
    {

        private readonly ImmutableArray<ITargetProvider> m_providers;


        public TargetProviderList(params ITargetProvider[] _collection)
            : this((IEnumerable<ITargetProvider>) _collection) { }

        public TargetProviderList(IEnumerable<ITargetProvider> _providers)
        {
            m_providers = _providers.ToImmutableArray();
        }

        public ITargetProvider this[int _index] => m_providers[_index];

        public int Count => m_providers.Length;

        public IEnumerator<ITargetProvider> GetEnumerator() => ((IEnumerable<ITargetProvider>) m_providers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_providers).GetEnumerator();

        public Target? GetTarget(XamlInfo _xamlInfo)
            => m_providers.Select(_p => _p.GetTarget(_xamlInfo)).FirstOrDefault(_t => _t is not null);

    }

}
