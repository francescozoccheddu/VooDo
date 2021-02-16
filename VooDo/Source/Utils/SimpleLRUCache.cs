#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLRUCache
{

    public sealed class LRUCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {

        private const int c_defaultCapacity = 512;

        private readonly struct ValueHolder
        {

            internal readonly LinkedListNode<TKey> node;
            internal readonly TValue value;

            internal ValueHolder(LinkedListNode<TKey> _node, TValue _value)
            {
                node = _node;
                value = _value;
            }

        }

        private readonly LinkedList<TKey> m_keys;
        private readonly Dictionary<TKey, ValueHolder> m_dictionary;
        private int m_capacity;
        private TValue[] m_values;

        private IEnumerable<KeyValuePair<TKey, TValue>> m_Enumerable
            => m_dictionary.Select(_p => new KeyValuePair<TKey, TValue>(_p.Key, _p.Value.value));

        public LRUCache() : this(EqualityComparer<TKey>.Default) { }

        public LRUCache(IDictionary<TKey, TValue> _dictionary) : this(_dictionary, EqualityComparer<TKey>.Default) { }

        public LRUCache(IEqualityComparer<TKey> _comparer) : this(c_defaultCapacity, _comparer) { }

        public LRUCache(int _capacity) : this(_capacity, EqualityComparer<TKey>.Default) { }

        public LRUCache(IDictionary<TKey, TValue> _dictionary, IEqualityComparer<TKey> _comparer)
        {
            if (_dictionary is null)
            {
                throw new ArgumentNullException(nameof(_dictionary));
            }
            if (_comparer is null)
            {
                throw new ArgumentNullException(nameof(_comparer));
            }
            m_capacity = _dictionary.Count;
            m_keys = new LinkedList<TKey>();
            m_dictionary = new Dictionary<TKey, ValueHolder>(m_capacity);
            foreach (KeyValuePair<TKey, TValue> item in _dictionary)
            {
                LinkedListNode<TKey> node = m_keys.AddLast(item.Key);
                m_dictionary.Add(item.Key, new ValueHolder(node, item.Value));
            }
        }

        public LRUCache(int _capacity, IEqualityComparer<TKey> _comparer)
        {
            if (_capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_capacity), $"Capacity cannot be negative");
            }
            if (_comparer is null)
            {
                throw new ArgumentNullException(nameof(_comparer));
            }
            m_keys = new LinkedList<TKey>();
            m_dictionary = new Dictionary<TKey, ValueHolder>(_capacity, _comparer);
            m_capacity = _capacity;
        }

        public int Capacity
        {
            get => m_capacity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(Capacity)} cannot be negative");
                }
                m_capacity = value;
                Trim(m_capacity);
            }
        }

        public void Trim(int _maxCount)
        {
            if (_maxCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_maxCount), $"Max count cannot be negative");
            }
            if (_maxCount >= Count)
            {
                return;
            }
            if (_maxCount == 0)
            {
                Clear();
            }
            while (Count > _maxCount)
            {
                TKey key = m_keys.First.Value;
                m_keys.RemoveFirst();
                _ = m_dictionary.Remove(key);
            }
            m_values = null;
        }

        public TValue this[TKey _key]
        {
            get => TryGetValue(_key, out TValue value) ? value : throw new KeyNotFoundException();
            set
            {
                if (m_dictionary.TryGetValue(_key, out ValueHolder holder))
                {
                    m_keys.Remove(holder.node);
                    m_keys.AddLast(holder.node);
                    m_dictionary[_key] = new ValueHolder(holder.node, value);
                    m_values = null;
                }
                else
                {
                    LinkedListNode<TKey> node = m_keys.AddLast(_key);
                    Trim(Capacity - 1);
                    m_dictionary.Add(_key, new ValueHolder(node, value));
                    m_values = null;
                }
            }
        }

        public int Count
            => m_dictionary.Count;

        public bool IsReadOnly
            => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
            => m_dictionary.Keys;

        public IEnumerable<TValue> Values
            => m_dictionary.Values.Select(_v => _v.value);

        public ICollection<TKey> Keys => m_dictionary.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (m_values is null)
                {
                    m_values = Values.ToArray();
                }
                return m_values;
            }
        }

        public void Add(TKey _key, TValue _value)
        {
            if (m_dictionary.ContainsKey(_key))
            {
                throw new ArgumentException("An argument with the same key already exists", nameof(_key));
            }
            LinkedListNode<TKey> node = m_keys.AddLast(_key);
            Trim(Capacity - 1);
            m_dictionary.Add(_key, new ValueHolder(node, _value));
            m_values = null;
        }

        public void Add(KeyValuePair<TKey, TValue> _item)
            => Add(_item.Key, _item.Value);

        public void Clear()
        {
            if (Count > 0)
            {
                m_values = null;
            }
            m_dictionary.Clear();
            m_keys.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> _item)
            => m_dictionary.TryGetValue(_item.Key, out ValueHolder holder) && EqualityComparer<TValue>.Default.Equals(_item.Value, holder.value);

        public bool ContainsKey(TKey _key)
            => m_dictionary.ContainsKey(_key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] _array, int _arrayIndex)
        {
            foreach (KeyValuePair<TKey, TValue> value in m_Enumerable)
            {
                _array[_arrayIndex++] = value;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => m_Enumerable.GetEnumerator();

        public bool TryGetValue(TKey _key, out TValue _value)
        {
            if (m_dictionary.TryGetValue(_key, out ValueHolder holder))
            {
                _value = holder.value;
                m_keys.Remove(holder.node);
                m_keys.AddLast(holder.node);
                return true;
            }
            else
            {
                _value = default;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool Remove(TKey _key)
        {
            if (m_dictionary.TryGetValue(_key, out ValueHolder holder))
            {
                m_keys.Remove(holder.node);
                _ = m_dictionary.Remove(_key);
                m_values = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> _item)
        {
            if (m_dictionary.TryGetValue(_item.Key, out ValueHolder holder) && EqualityComparer<TValue>.Default.Equals(_item.Value, holder.value))
            {
                m_keys.Remove(holder.node);
                _ = m_dictionary.Remove(_item.Key);
                m_values = null;
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
