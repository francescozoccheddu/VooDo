using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;

namespace VooDo.WinUI.Options
{

    public sealed class ConstantAttribute : AssemblyTagAttribute
    {

        private readonly Constant m_constant;

        internal ConstantAttribute(Identifier _name, object _value) : this(_value.GetType(), _name, _value) { }

        internal ConstantAttribute(UnresolvedType _type, Identifier _name, object? _value)
        {
            m_constant = new Constant(_type, _name, _value);
        }

        internal static ImmutableArray<Constant> Resolve(IEnumerable<ConstantAttribute> _attributes)
            => _attributes.Select(_a => _a.m_constant).ToImmutableArray();

    }


}
