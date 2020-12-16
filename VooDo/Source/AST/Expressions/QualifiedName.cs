using VooDo.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.AST
{

    public sealed class QualifiedName : ASTBase
    {

        internal QualifiedName(Name _base) : this(new Name[] { _base }) { }

        internal QualifiedName(IEnumerable<Name> _path)
        {
            Ensure.NonNull(_path, nameof(_path));
            Ensure.NonNullItems(_path, nameof(_path));
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

        public sealed override string Code => string.Join('@', m_path.Select(_n => _n.Code));

        public sealed override bool Equals(object _obj) => _obj is QualifiedName name && m_path.SequenceEqual(name.m_path);
        public sealed override int GetHashCode() => Identity.CombineHash(m_path);

        #endregion

    }

}
