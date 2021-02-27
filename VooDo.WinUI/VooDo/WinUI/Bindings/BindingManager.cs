using System;
using System.Collections.Generic;

using VooDo.Utils;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Bindings
{

    public static class BindingManager
    {

        private sealed class Map<TKey> where TKey : notnull
        {

            private static readonly IReadOnlySet<Binding> s_empty = new HashSet<Binding>().AsReadOnly();

            private readonly Dictionary<TKey, HashSet<Binding>> m_dictionary;
            private readonly Func<Binding, TKey> m_keySelector;

            internal Map(Func<Binding, TKey> _keySelector) : this(_keySelector, EqualityComparer<TKey>.Default) { }

            internal Map(Func<Binding, TKey> _keySelector, IEqualityComparer<TKey> _comparer)
            {
                m_dictionary = new Dictionary<TKey, HashSet<Binding>>(_comparer);
                m_keySelector = _keySelector;
            }

            internal IReadOnlySet<Binding> this[TKey _key]
                => m_dictionary.GetValueOrDefault(_key)?.AsReadOnly() ?? s_empty;

            internal bool Remove(Binding _binding)
            {
                TKey key = m_keySelector(_binding);
                if (m_dictionary.TryGetValue(key, out HashSet<Binding>? set))
                {
                    if (set.Remove(_binding))
                    {
                        if (set.Count == 0)
                        {
                            m_dictionary.Remove(key);
                        }
                        return true;
                    }
                }
                return false;
            }

            internal bool Add(Binding _binding)
            {
                TKey key = m_keySelector(_binding);
                if (m_dictionary.TryGetValue(key, out HashSet<Binding>? set))
                {
                    return set.Add(_binding);
                }
                else
                {
                    m_dictionary[key] = new HashSet<Binding>()
                    {
                        _binding
                    };
                    return true;
                }
            }

            internal bool Contains(Binding _binding)
                => m_dictionary.GetValueOrDefault(m_keySelector(_binding))?.Contains(_binding) ?? false;

        }

        private static readonly Map<string> s_tagMap = new(_b => _b.SourceTag);
        private static readonly Map<object> s_ownerMap = new(_b => _b.XamlOwner, ReferenceEqualityComparer.Instance);
        private static readonly Map<object> s_rootMap = new(_b => _b.XamlRoot, ReferenceEqualityComparer.Instance);
        private static readonly Map<string> s_sourcePath = new(_b => _b.SourcePath, StringComparer.FromComparison(FilePaths.SystemComparison));
        private static readonly HashSet<Binding> s_bindings = new();

        public static IReadOnlySet<Binding> Bindings => s_bindings.AsReadOnly();

        public static IReadOnlySet<Binding> GetByTag(string _tag)
            => s_tagMap[_tag];

        public static IReadOnlySet<Binding> GetByXamlOwner(object _owner)
            => s_ownerMap[_owner];

        public static IReadOnlySet<Binding> GetByXamlRoot(object _root)
            => s_rootMap[_root];

        public static IReadOnlySet<Binding> GetByXamlPath(string _file)
            => s_sourcePath[FilePaths.Normalize(_file)];

        public static bool Add(Binding _binding)
        {
            if (s_bindings.Add(_binding))
            {
                s_tagMap.Add(_binding);
                s_ownerMap.Add(_binding);
                s_rootMap.Add(_binding);
                s_sourcePath.Add(_binding);
                _binding.OnAdd();
                return true;
            }
            return false;
        }

        public static bool Remove(Binding _binding)
        {
            if (s_bindings.Remove(_binding))
            {
                s_tagMap.Remove(_binding);
                s_ownerMap.Remove(_binding);
                s_rootMap.Remove(_binding);
                s_sourcePath.Remove(_binding);
                _binding.OnRemove();
                return true;
            }
            return false;
        }

    }

}
