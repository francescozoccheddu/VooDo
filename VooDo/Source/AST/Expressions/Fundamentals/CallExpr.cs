using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CallExpr : Expr
    {

        internal CallExpr(Expr _callable, IEnumerable<Expr> _arguments)
        {
            Ensure.NonNull(_callable, nameof(_callable));
            Ensure.NonNull(_arguments, nameof(_arguments));
            Ensure.NonNullItems(_arguments, nameof(_arguments));
            Callable = _callable;
            m_arguments = _arguments.ToArray();
        }

        private readonly Expr[] m_arguments;

        public Expr Callable { get; }
        public IEnumerable<Expr> Arguments => m_arguments;

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Precedence => 0;

        public sealed override string Code =>
               $"{Callable.LeftCode(Precedence)}({m_arguments.ArgumentsListCode()})";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is CallExpr expr && Callable.Equals(expr.Callable) && Arguments.SequenceEqual(expr.Arguments);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Callable, Identity.CombineHash(m_arguments));

        #endregion

    }

}
