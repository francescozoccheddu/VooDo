using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS.Exceptions
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

    }
}
