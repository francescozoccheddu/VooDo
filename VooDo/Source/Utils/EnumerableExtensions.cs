

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Utils
{
    public static class EnumerableExtensions
    {

        public static ImmutableArray<TItem> EmptyIfDefault<TItem>(this ImmutableArray<TItem> _array)
            => _array.IsDefault ? ImmutableArray.Create<TItem>() : _array;

        internal static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem>? _enumerable)
            => _enumerable ?? Enumerable.Empty<TItem>();

        internal static bool AnyNull<TItem>(this IEnumerable<TItem?> _enumerable) where TItem : class
            => _enumerable.Any(_i => _i is null);

        internal static bool FirstDuplicate<TItem>(this IEnumerable<TItem> _enumerable, out TItem? _item, IEqualityComparer<TItem>? _equalityComparer = null)
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

        internal static bool AnyDuplicate<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem>? _equalityComparer = null)
            => FirstDuplicate(_enumerable, out _, _equalityComparer);

        internal static ImmutableArray<TValue> NonNull<TValue>(this ImmutableArray<TValue?> _array) where TValue : class
            => (_array.AnyNull()
            ? throw new NullReferenceException()
            : _array)!;

        internal static TValue NonNull<TValue>(this TValue? _value) where TValue : class
            => _value is null
            ? throw new NullReferenceException()
            : _value;

        internal static IEnumerable<(TItem item, int index)> Enumerate<TItem>(this IEnumerable<TItem> _items)
        {
            int i = 0;
            foreach (TItem? item in _items)
            {
                yield return (item, i++);
            }
        }

        internal static IEnumerable<TOutItem> SelectIndexed<TItem, TOutItem>(this IEnumerable<TItem> _items, Func<TItem, int, TOutItem> _map)
        {
            int i = 0;
            foreach (TItem? item in _items)
            {
                yield return _map(item, i);
            }
        }

        internal static IEnumerable<TOutItem> SelectNonNull<TItem, TOutItem>(this IEnumerable<TItem?> _items, Func<TItem, TOutItem?> _map)
        {
            foreach (TItem item in _items)
            {
                TOutItem? mapped = _map(item!);
                if (mapped is not null)
                {
                    yield return mapped;
                }
            }
        }

        internal static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, TItem?> _map)
            => _array.Map(_map, new Identity.ReferenceComparer<TItem?>());

        internal static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, TItem?> _map, IEqualityComparer<TItem?> _comparer)
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

        internal static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, object?> _map)
            => _array.Map(_map, new Identity.ReferenceComparer<TItem?>());

        internal static ImmutableArray<TItem?> Map<TItem>(this ImmutableArray<TItem> _array, Func<TItem, object?> _map, IEqualityComparer<TItem?> _comparer)
            => _array.Map(_a => (TItem?) _map(_a), _comparer);

    }
}
