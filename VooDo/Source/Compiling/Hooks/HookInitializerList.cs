
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace VooDo.Hooks
{

    public sealed class HookInitializerList : IReadOnlyList<IHookInitializerProvider>, IHookInitializerProvider
    {

        private ImmutableArray<IHookInitializerProvider> m_hookInitializers;

        public HookInitializerList(IEnumerable<IHookInitializerProvider> _collection)
        {
            m_hookInitializers = _collection.ToImmutableArray();
        }

        public IHookInitializerProvider this[int _index] => m_hookInitializers[_index];

        public int Count => m_hookInitializers.Length;

        public IEnumerator<IHookInitializerProvider> GetEnumerator() => ((IEnumerable<IHookInitializerProvider>) m_hookInitializers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_hookInitializers).GetEnumerator();

    }

}
