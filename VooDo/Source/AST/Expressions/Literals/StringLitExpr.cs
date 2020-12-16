
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

using VooDo.Utils;

namespace VooDo.AST.Expressions.Literals
{

    public sealed class StringLitExpr : LitExpr<string>
    {

        internal StringLitExpr(string _value) : base(_value)
        {
            Ensure.NonNull(_value, nameof(_value));
        }

        #region Expr

        public sealed override string Code => Syntax.EscapeString(Literal);

        #endregion

    }

}
