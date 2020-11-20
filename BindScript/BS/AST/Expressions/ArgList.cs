using BS.AST.Expressions;
using BS.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BS.AST.Expressions
{

    public sealed class ArgList : ReadOnlyCollection<Expr>, Syntax.ICode
    {

        internal ArgList(params Expr[] _arguments) : this((IEnumerable<Expr>) _arguments)
        { }

        internal ArgList(IEnumerable<Expr> _arguments) : base(_arguments.ToArray())
        { }

        public string Code => Syntax.FormatArgList(this.Select(_a => _a.Code));

    }

}
