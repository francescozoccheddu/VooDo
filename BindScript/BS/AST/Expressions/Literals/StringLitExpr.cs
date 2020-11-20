
using BS.Exceptions;

using System;

namespace BS.AST.Expressions.Literals
{

    public sealed class StringLitExpr : LitExpr<string>
    {

        internal StringLitExpr(string _value) : base(_value)
        {
            Ensure.NonNull(_value, nameof(_value));
        }

        public sealed override string Code => Syntax.FormatLitExp(Literal);
    }

}
