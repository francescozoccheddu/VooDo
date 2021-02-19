using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.WinUI.Attributes
{

    public sealed class UsingNamespaceAttribute : AssemblyTagAttribute
    {

        private readonly string m_namespace;

        public string? Alias { get; set; }

        internal UsingNamespaceAttribute(string _namespace)
        {
            m_namespace = _namespace;
        }

        internal static ImmutableArray<(string name, string? alias)> Resolve(IEnumerable<UsingNamespaceAttribute> _attributes)
            => _attributes.Select(_a => (_a.m_namespace, _a.Alias)).ToImmutableArray();

    }


}
