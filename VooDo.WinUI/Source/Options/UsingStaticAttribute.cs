using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;

namespace VooDo.WinUI.Options
{

    public sealed class UsingStaticAttribute : AssemblyTagAttribute
    {

        private readonly UnresolvedType m_type;

        internal UsingStaticAttribute(QualifiedType _type)
        {
            m_type = new UnresolvedType(_type);
        }

        internal UsingStaticAttribute(Type _type)
        {
            m_type = new UnresolvedType(_type);
        }

        internal static ImmutableArray<UnresolvedType> Resolve(IEnumerable<UsingStaticAttribute> _attributes)
            => _attributes.Select(_a => _a.m_type).ToImmutableArray();

    }


}
