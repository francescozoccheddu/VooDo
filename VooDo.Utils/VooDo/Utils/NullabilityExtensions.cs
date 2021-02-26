using System;
using System.Collections.Immutable;

namespace VooDo.Utils
{

    public static class NullabilityExtensions
    {

        public static ImmutableArray<TValue> NonNull<TValue>(this ImmutableArray<TValue?> _array) where TValue : class
            => (_array.AnyNull()
            ? throw new NullReferenceException()
            : _array)!;

        public static TValue NonNull<TValue>(this TValue? _value) where TValue : class
            => _value is null
            ? throw new NullReferenceException()
            : _value;

    }

}
