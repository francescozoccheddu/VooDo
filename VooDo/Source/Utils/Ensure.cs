using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.Utils
{
    internal static class Ensure
    {

        internal static void NonNull(object _value, string _argumentName)
        {
            if (_value == null)
            {
                throw new ArgumentNullException(_argumentName);
            }
        }

        internal static void NonNullItems(IEnumerable _value, string _argumentName)
        {
            foreach (object item in _value)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(_argumentName);
                }
            }
        }

    }
}
