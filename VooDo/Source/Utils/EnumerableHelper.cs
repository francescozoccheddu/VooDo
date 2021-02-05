using System.Collections.Generic;
using System.Linq;

namespace VooDo.Utils
{
    internal static class EnumerableHelper
    {

        internal static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem> _enumerable)
            => _enumerable ?? Enumerable.Empty<TItem>();

        internal static bool AnyNull<TItem>(this IEnumerable<TItem> _enumerable) where TItem : class
            => _enumerable.Any(_i => _i == null);


    }
}
