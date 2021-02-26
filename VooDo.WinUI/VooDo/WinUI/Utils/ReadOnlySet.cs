using System.Collections;
using System.Collections.Generic;

namespace VooDo.WinUI.Utils
{

    public static class ReadOnlySetExtensions
    {

        public static ReadOnlySet<TValue> AsReadOnly<TValue>(this ISet<TValue> _set) => new ReadOnlySet<TValue>(_set);

    }

    public sealed class ReadOnlySet<TValue> : IReadOnlySet<TValue>
    {

        private readonly ISet<TValue> m_set;

        public ReadOnlySet(ISet<TValue> _set)
        {
            m_set = _set;
        }

        public int Count => m_set.Count;
        public bool IsReadOnly => true;
        public bool Contains(TValue _item) => m_set.Contains(_item);
        public void CopyTo(TValue[] _array, int _arrayIndex) => m_set.CopyTo(_array, _arrayIndex);
        public IEnumerator<TValue> GetEnumerator() => m_set.GetEnumerator();
        public bool IsProperSubsetOf(IEnumerable<TValue> _other) => m_set.IsProperSubsetOf(_other);
        public bool IsProperSupersetOf(IEnumerable<TValue> _other) => m_set.IsProperSupersetOf(_other);
        public bool IsSubsetOf(IEnumerable<TValue> _other) => m_set.IsSubsetOf(_other);
        public bool IsSupersetOf(IEnumerable<TValue> _other) => m_set.IsSupersetOf(_other);
        public bool Overlaps(IEnumerable<TValue> _other) => m_set.Overlaps(_other);
        public bool SetEquals(IEnumerable<TValue> _other) => m_set.SetEquals(_other);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_set).GetEnumerator();

    }

}