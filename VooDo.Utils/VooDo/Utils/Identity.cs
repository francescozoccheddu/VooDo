﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VooDo.Utils
{

    public static class Identity
    {

        public sealed class ReferenceComparer<T> : IEqualityComparer<T>
        {

            public static ReferenceComparer<T> Instance { get; } = new ReferenceComparer<T>();

            private ReferenceComparer() { }

            public bool Equals(T? _x, T? _y) => ReferenceEquals(_x, _y);
            public int GetHashCode(T? _obj) => RuntimeHelpers.GetHashCode(_obj);
        }

        public static int CombineHash(params object?[] _objs) => CombineHashes(_objs);

        public static int CombineHashes(IEnumerable _objs)
        {
            int hashCode = 579313498;
            foreach (object obj in _objs)
            {
                hashCode = (hashCode * -1521134295) + (obj?.GetHashCode() ?? 0);
            }
            return hashCode;
        }

    }

}