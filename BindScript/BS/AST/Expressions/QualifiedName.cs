
using BS.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST
{

    public sealed class QualifiedName : Syntax.ICode
    {

        public static implicit operator QualifiedName(Name _base) => new QualifiedName(_base);

        internal QualifiedName(Name _base) : this(new Name[] { _base }) { }

        internal QualifiedName(params Name[] _path) : this((IEnumerable<Name>) _path) { }

        internal QualifiedName(IEnumerable<Name> _path)
        {
            Ensure.NonNull(_path, nameof(_path));
            m_path = _path.ToArray();
            if (m_path.Length < 1)
            {
                throw new ArgumentException("Empty name list", nameof(_path));
            }
        }

        private readonly Name[] m_path;

        public bool IsQualified => m_path.Length > 1;
        public Name BaseName => m_path.Last();
        public IEnumerable<Name> NameSpace => m_path.SkipLast(1);

        public string Code => Syntax.FormatQualName(m_path.Select(_s => _s.Code));

    }

}
