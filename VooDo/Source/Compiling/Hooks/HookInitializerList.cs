
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Hooks
{

    public sealed class HookInitializerList : IReadOnlyList<IHookInitializerProvider>, IHookInitializerProvider, IEquatable<HookInitializerList?>
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

        public override bool Equals(object? _obj) => Equals(_obj as HookInitializerList);
        public bool Equals(HookInitializerList? _other) => _other is not null && m_hookInitializers.SequenceEqual(_other.m_hookInitializers);
        public static bool operator ==(HookInitializerList? _left, HookInitializerList? _right) => _left is not null && _left.Equals(_right);
        public static bool operator !=(HookInitializerList? _left, HookInitializerList? _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHashes(m_hookInitializers);
    }

}
