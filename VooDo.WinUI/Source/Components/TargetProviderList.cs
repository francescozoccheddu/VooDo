using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class TargetProviderList<TTarget> : IReadOnlyList<ITargetProvider<TTarget>>, ITargetProvider<TTarget> where TTarget : ITarget
    {

        private readonly ImmutableArray<ITargetProvider<TTarget>> m_providers;

        public TargetProviderList(IEnumerable<ITargetProvider<TTarget>> _providers)
        {
            m_providers = _providers.ToImmutableArray();
        }

        public ITargetProvider<TTarget> this[int _index] => m_providers[_index];

        public int Count => m_providers.Length;

        public IEnumerator<ITargetProvider<TTarget>> GetEnumerator() => ((IEnumerable<ITargetProvider<TTarget>>) m_providers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_providers).GetEnumerator();

        public TTarget? GetTarget(XamlInfo _xamlInfo)
            => m_providers.Select(_p => _p.GetTarget(_xamlInfo)).FirstOrDefault(_t => _t is not null);

    }

}
