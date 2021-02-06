#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Utils
{
    internal static class EnumerableHelper
    {

        internal static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem>? _enumerable)
            => _enumerable ?? Enumerable.Empty<TItem>();

        internal static bool AnyNull<TItem>(this IEnumerable<TItem> _enumerable) where TItem : class
            => _enumerable.Any(_i => _i == null);

        internal static bool AnyDuplicate<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem> _equalityComparer = null)
        {
            ImmutableHashSet<TItem> set = _enumerable.ToImmutableHashSet(_equalityComparer ?? EqualityComparer<TItem>.Default);
            return set.Count < _enumerable.Count();
        }

        internal static ImmutableHashSet<TItem> DistintToImmutableHashSet<TItem>(this IEnumerable<TItem> _enumerable, IEqualityComparer<TItem> _equalityComparer = null)
        {
            ImmutableHashSet<TItem> set = _enumerable.ToImmutableHashSet(_equalityComparer ?? EqualityComparer<TItem>.Default);
            if (set.Count < _enumerable.Count())
            {
                throw new ArgumentException("Duplicate found", nameof(_enumerable));
            }
            return set;
        }

    }
}
