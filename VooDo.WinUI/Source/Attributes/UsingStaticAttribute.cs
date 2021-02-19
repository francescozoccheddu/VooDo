using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.WinUI.Attributes
{

    public sealed class UsingStaticAttribute : AssemblyTagAttribute
    {

        private readonly Type m_type;

        internal UsingStaticAttribute(Type _type)
        {
            m_type = _type;
        }

        internal static ImmutableArray<Type> Resolve(IEnumerable<UsingStaticAttribute> _attributes)
            => _attributes.Select(_a => _a.m_type).ToImmutableArray();

    }


}
