using BS.AST.Expressions;
using BS.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST.Expressions
{

    public sealed class ArgList : IReadOnlyList<Expr>
    {

        internal ArgList(IEnumerable<Expr> _arguments)
        {
            Ensure.NonNull(_arguments, nameof(_arguments));
            m_arguments = _arguments.ToArray();
        }

        private readonly Expr[] m_arguments;

        public int Count => m_arguments.Length;

        public Expr this[int _index] => m_arguments[_index];

        public IEnumerator<Expr> GetEnumerator() => ((IEnumerable<Expr>) m_arguments).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_arguments.GetEnumerator();

    }

}
