
using BS.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BS.AST
{

    public sealed class QualifiedName : ASTBase
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

        #region ASTBase

        public sealed override string Code => Syntax.FormatQualName(m_path.Select(_s => _s.Code));

        public sealed override bool Equals(object _obj) => _obj is QualifiedName name && m_path.Equals(name.m_path);
        public sealed override int GetHashCode() => m_path.GetHashCode();

        #endregion

    }

}
