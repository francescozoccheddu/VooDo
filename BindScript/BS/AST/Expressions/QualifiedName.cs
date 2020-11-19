
using BS.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST
{

    public sealed class QualifiedName : IReadOnlyList<Name>
    {

        internal QualifiedName(IEnumerable<Name> _path)
        {
            Ensure.NonNull(_path, nameof(_path));
            m_path = _path.ToArray();
            if (Count < 1)
            {
                throw new ArgumentException("Empty name list", nameof(_path));
            }
        }

        private readonly Name[] m_path;

        public bool IsQualified => Count > 1;
        public Name Final => this.Last();
        public IEnumerable<Name> Path => this.SkipLast(1);

        public int Count => m_path.Length;

        public Name this[int _index] => m_path[_index];

        public IEnumerator<Name> GetEnumerator() => ((IEnumerable<Name>) m_path).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_path.GetEnumerator();
    }

}
