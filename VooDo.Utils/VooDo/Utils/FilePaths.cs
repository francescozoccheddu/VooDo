using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Utils
{

    public static class FilePaths
    {

        public static StringComparer Comparer { get; } = new PathComparer();
        public static StringComparison SystemComparison { get; } = StringComparison.OrdinalIgnoreCase;

        private sealed class PathComparer : StringComparer
        {
            public override int Compare(string _x, string _y) => string.Compare(Normalize(_x), Normalize(_y), SystemComparison);
            public override bool Equals(string _x, string _y) => AreEqual(_x, _y);
            public override int GetHashCode(string _obj) => Normalize(_obj).GetHashCode();
        }

        public static TItem? FirstWithFile<TItem>(this IEnumerable<TItem?> _items, string _file, Func<TItem?, string?> _fileSelector)
        {
            _file = Normalize(_file);
            return _items.FirstOrDefault(_i => _fileSelector(_i) is string other && AreEqualNormalized(Normalize(other), _file));
        }

        public static IEnumerable<TItem?> WhereWithFile<TItem>(this IEnumerable<TItem?> _items, string _file, Func<TItem?, string?> _fileSelector)
        {
            _file = Normalize(_file);
            return _items.Where(_i => _fileSelector(_i) is string other && AreEqualNormalized(Normalize(other), _file));
        }

        public static TItem? SingleWithFile<TItem>(this IEnumerable<TItem?> _items, string _file, Func<TItem?, string?> _fileSelector)
        {
            _file = Normalize(_file);
            return _items.SingleOrDefault(_i => _fileSelector(_i) is string other && AreEqualNormalized(Normalize(other), _file));
        }

        private static bool AreEqualNormalized(string _a, string _b)
            => _a.Equals(_b, SystemComparison);

        public static bool AreEqual(string _a, string _b)
            => AreEqualNormalized(Normalize(_a), Normalize(_b));

        public static string Normalize(string _path)
            => Uri.UnescapeDataString(new Uri(_path).AbsolutePath);

        public static string? NormalizeOrNull(string? _path)
            => Uri.UnescapeDataString(new Uri(_path).AbsolutePath);

    }

}
