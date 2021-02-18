using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.AST.Names;
using VooDo.Compiling;

namespace VooDo.WinUI.Attributes
{

    public sealed class ReferenceAttribute : AssemblyTagAttribute
    {

        private readonly Reference m_reference;

        internal ReferenceAttribute(string _filePath, params string[] _aliases)
        {
            m_reference = Reference.FromFile(_filePath, _aliases.Select(_a => new Identifier(_a)));
        }

        internal ReferenceAttribute(Assembly _assembly, params string[] _aliases)
        {
            m_reference = Reference.FromAssembly(_assembly, _aliases.Select(_a => new Identifier(_a)));
        }

        internal static ImmutableArray<Reference> Resolve(IEnumerable<ReferenceAttribute> _attributes)
            => _attributes.Select(_a => _a.m_reference).ToImmutableArray();

    }


}
