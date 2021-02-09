

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Utils
{
    internal static class EnumerableHelper
    {

        internal static ImmutableArray<TItem> EmptyIfDefault<TItem>(this ImmutableArray<TItem> _array)
            => _array.IsDefault ? ImmutableArray.Create<TItem>() : _array;

        internal static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem>? _enumerable)
            => _enumerable ?? Enumerable.Empty<TItem>();

        internal static bool AnyNull<TItem>(this IEnumerable<TItem?> _enumerable) where TItem : class
            => _enumerable.Any(_i => _i is null);

        internal static bool AnyDuplicate<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem>? _equalityComparer = null)
        {
            ImmutableHashSet<TItem> set = _enumerable.ToImmutableHashSet(_equalityComparer ?? EqualityComparer<TItem>.Default);
            return set.Count < _enumerable.Count();
        }

        internal static ImmutableHashSet<TItem> DistintToImmutableHashSet<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem>? _equalityComparer = null)
        {
            ImmutableHashSet<TItem> set = _enumerable.ToImmutableHashSet(_equalityComparer ?? EqualityComparer<TItem>.Default);
            if (set.Count < _enumerable.Count())
            {
                throw new ArgumentException("Duplicate found", nameof(_enumerable));
            }
            return set;
        }

        internal static TSeq NonEmpty<TSeq, TItem>(this TSeq _enumerable, string? _parameter = null) where TSeq : IEnumerable<TItem>
            => _enumerable.Assert(_a => _a.Any(), "Empty sequence", _parameter);

        internal static ImmutableArray<TItem> NonEmpty<TItem>(this ImmutableArray<TItem> _array, string? _parameter = null)
            => _array.Assert(_a => !_a.IsDefaultOrEmpty, "Empty array", _parameter);

        internal static TSeq MoreThanOne<TSeq, TItem>(this TSeq _enumerable, string? _parameter = null) where TSeq : IEnumerable<TItem>
            => _enumerable.Assert(_a => _a.Count() > 1, "Sequence must contain at least two elements", _parameter);

        internal static TSeq AssertAll<TSeq, TItem>(this TSeq _enumerable, Func<TItem, bool> _predicate, string _message = "Assertion failed", string? _parameter = null) where TSeq : IEnumerable<TItem>
            => _enumerable.Assert(_a => _a.All(_predicate), _message, _parameter);

        internal static TValue Assert<TValue>(this TValue _value, Func<TValue, bool> _predicate, string _message = "Assertion failed", string? _parameter = null)
            => !_predicate(_value)
            ? throw new ArgumentException(_message, _parameter)
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

    }
}
