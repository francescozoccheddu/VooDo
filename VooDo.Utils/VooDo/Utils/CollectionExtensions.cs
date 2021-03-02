using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Utils
{

    public static class CollectionExtensions
    {

        public static IEnumerable<TValue> Singleton<TValue>(TValue _value)
        {
            yield return _value;
        }

        public static IEnumerable<TItem> SkipLast<TItem>(this IEnumerable<TItem> _items)
        {
            IEnumerator<TItem> enumerator = _items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }
            TItem last = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return last;
                last = enumerator.Current;
            }
        }

        public static ImmutableArray<TItem> EmptyIfDefault<TItem>(this ImmutableArray<TItem> _array)
            => _array.IsDefault ? ImmutableArray.Create<TItem>() : _array;

        public static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem>? _enumerable)
            => _enumerable ?? Enumerable.Empty<TItem>();

        public static bool AnyNull<TItem>(this IEnumerable<TItem?> _enumerable) where TItem : class
            => _enumerable.Any(_i => _i is null);

        public static bool FirstDuplicate<TItem>(this IEnumerable<TItem> _enumerable, out TItem? _item, IEqualityComparer<TItem>? _equalityComparer = null)
        {
            _item = default;
            HashSet<TItem> set = new(_equalityComparer ?? EqualityComparer<TItem>.Default);
            foreach (TItem item in _enumerable)
            {
                if (!set.Add(item))
                {
                    _item = item;
                    return true;
                }
            }
            return false;
        }

        public static bool AnyDuplicate<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem>? _equalityComparer = null)
            => FirstDuplicate(_enumerable, out _, _equalityComparer);

        public static IEnumerable<(TItem item, int index)> Enumerate<TItem>(this IEnumerable<TItem> _items)
        {
            int i = 0;
            foreach (TItem? item in _items)
            {
                yield return (item, i++);
            }
        }

        public static IEnumerable<TOutItem> SelectIndexed<TItem, TOutItem>(this IEnumerable<TItem> _items, Func<TItem, int, TOutItem> _map)
        {
            int i = 0;
            foreach (TItem? item in _items)
            {
                yield return _map(item, i++);
            }
        }

        public static IEnumerable<TOutItem> SelectNonNull<TItem, TOutItem>(this IEnumerable<TItem> _items, Func<TItem, TOutItem?> _map)
        {
            foreach (TItem item in _items)
            {
                TOutItem? mapped = _map(item);
                if (mapped is not null)
                {
                    yield return mapped;
                }
            }
        }

        public static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, TItem?> _map)
            => _array.Map(_map, Identity.ReferenceComparer<TItem?>.Instance);

        public static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, TItem?> _map, IEqualityComparer<TItem?> _comparer)
        {
            TItem?[]? newItems = null;
            int i;
            for (i = 0; i < _array.Length; i++)
            {
                TItem? newItem = _map(_array[i]);
                if (!_comparer.Equals(newItem, _array[i]))
                {
                    newItems = new TItem[_array.Length];
                    for (int c = 0; c < i; c++)
                    {
                        newItems[c] = _array[c];
                    }
                    newItems[i] = newItem;
                    break;
                }
            }
            while (++i < _array.Length)
            {
                newItems![i] = _map(_array[i]);
            }
            if (newItems is null)
            {
                return _array!;
            }
            else
            {
                return newItems.ToImmutableArray();
            }
        }

        public static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, object?> _map)
            => _array.Map(_map, Identity.ReferenceComparer<TItem?>.Instance);

        public static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, object?> _map, IEqualityComparer<TItem?> _comparer)
            => _array.Map(_a => (TItem?)_map(_a), _comparer);

    }

}
