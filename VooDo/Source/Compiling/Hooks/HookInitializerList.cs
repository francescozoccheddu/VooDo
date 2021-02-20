
using Microsoft.CodeAnalysis;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Utils;

namespace VooDo.Hooks
{

    public sealed class HookInitializerList : IReadOnlyList<IHookInitializer>, IHookInitializer, IEquatable<HookInitializerList?>
    {

        private ImmutableArray<IHookInitializer> m_hookInitializers;

        public HookInitializerList(IEnumerable<IHookInitializer> _collection)
        {
            m_hookInitializers = _collection.ToImmutableArray();
        }

        public IHookInitializer this[int _index] => m_hookInitializers[_index];

        public int Count => m_hookInitializers.Length;

        public IEnumerator<IHookInitializer> GetEnumerator() => ((IEnumerable<IHookInitializer>) m_hookInitializers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_hookInitializers).GetEnumerator();

        public override bool Equals(object? _obj) => Equals(_obj as HookInitializerList);
        public bool Equals(HookInitializerList? _other) => _other is not null && m_hookInitializers.SequenceEqual(_other.m_hookInitializers);
        public static bool operator ==(HookInitializerList? _left, HookInitializerList? _right) => _left is not null && _left.Equals(_right);
        public static bool operator !=(HookInitializerList? _left, HookInitializerList? _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHashes(m_hookInitializers);
        public Expression? GetInitializer(ISymbol _symbol)
            => m_hookInitializers.Select(_h => _h.GetInitializer(_symbol)).FirstOrDefault(_h => _h is not null);

    }

}
