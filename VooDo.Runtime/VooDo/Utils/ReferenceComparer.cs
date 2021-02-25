using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VooDo.Utils
{

    internal sealed class ReferenceComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T? _x, T? _y) => ReferenceEquals(_x, _y);
        public int GetHashCode(T? _obj) => RuntimeHelpers.GetHashCode(_obj);
    }

}