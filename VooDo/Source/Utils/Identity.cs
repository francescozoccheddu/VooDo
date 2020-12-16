using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.Utils
{
    internal static class Identity
    {

        internal static int CombineHash(params object[] _objs)
        {
            int hashCode = 579313498;
            foreach (object obj in _objs)
            {
                hashCode = hashCode * -1521134295 + (obj?.GetHashCode() ?? 0);
            }
            return hashCode;
        }

        internal static bool AreEqual(object _a, object _b)
            => ReferenceEquals(_a, _b) || (_a?.Equals(_b) ?? false);

    }
}
