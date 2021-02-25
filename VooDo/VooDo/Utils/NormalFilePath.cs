using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Utils
{

    public static class NormalFilePath
    {

        public static IEqualityComparer<string> Comparer { get; } = new PathComparer();

        private sealed class PathComparer : IEqualityComparer<string>
        {
            public bool Equals(string _x, string _y) => AreEqual(_x, _y);
            public int GetHashCode(string _obj) => Normalize(_obj).GetHashCode();
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
            => _a.Equals(_b, StringComparison.OrdinalIgnoreCase);

        public static bool AreEqual(string _a, string _b)
            => AreEqualNormalized(Normalize(_a), Normalize(_b));

        public static string Normalize(string _path)
            => Uri.UnescapeDataString(new Uri(_path).AbsolutePath);

        public static string? NormalizeOrNull(string? _path)
            => Uri.UnescapeDataString(new Uri(_path).AbsolutePath);

    }

}
