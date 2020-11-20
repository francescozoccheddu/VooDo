using BS.AST.Expressions;
using BS.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST.Expressions
{

    public sealed class ArgList : ASTBase, IReadOnlyList<Expr>
    {

        internal ArgList(params Expr[] _arguments) : this((IEnumerable<Expr>) _arguments)
        { }

        internal ArgList(IEnumerable<Expr> _arguments)
        {
            Ensure.NonNull(_arguments, nameof(_arguments));
            m_arguments = _arguments.ToArray();
        }

        private readonly Expr[] m_arguments;

        #region ASTBase

        public sealed override string Code => Syntax.FormatArgList(this.Select(_a => _a.Code));

        public sealed override bool Equals(object _obj) => _obj is ArgList argList && m_arguments.Equals(argList.m_arguments);
        public sealed override int GetHashCode() => m_arguments.GetHashCode();

        #endregion

        #region IReadOnlyList<Expr>

        public int Count => m_arguments.Length;

        public Expr this[int _index] => m_arguments[_index];

        public IEnumerator<Expr> GetEnumerator() => ((IEnumerable<Expr>) m_arguments).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_arguments.GetEnumerator();

        #endregion

    }

}
