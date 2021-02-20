using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.WinUI.Options
{

    public sealed class UsingNamespaceAttribute : AssemblyTagAttribute
    {

        private readonly UsingNamespace m_namespace;

        internal UsingNamespaceAttribute(UsingNamespace _usingNamespace)
        {
            m_namespace = _usingNamespace;
        }

        internal static ImmutableArray<UsingNamespace> Resolve(IEnumerable<UsingNamespaceAttribute> _attributes)
            => _attributes.Select(_a => _a.m_namespace).ToImmutableArray();

    }


}
