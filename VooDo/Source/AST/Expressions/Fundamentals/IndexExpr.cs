using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class IndexExpr : Expr
    {

        internal IndexExpr(Expr _indexable, IEnumerable<Expr> _index)
        {
            Ensure.NonNull(_indexable, nameof(_indexable));
            Ensure.NonNull(_index, nameof(_index));
            Ensure.NonNullItems(_index, nameof(_index));
            Indexable = _indexable;
            m_index = _index.ToArray();
            if (m_index.Length == 0)
            {
                throw new ArgumentException("Empty index", nameof(_index));
            }
        }

        private readonly Expr[] m_index;

        public Expr Indexable { get; }
        public IEnumerable<Expr> Index { get; }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        internal sealed override void Assign(Env _env, object _value)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 0;

        public sealed override string Code
            => $"{Indexable.LeftCode(Priority)}[{m_index.ArgumentsListCode()}]";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is IndexExpr expr && Indexable.Equals(expr.Indexable) && Index.Equals(expr.Index);

        public sealed override int GetHashCode()
            => Identity.CombineHash(Indexable, Index);

        #endregion

    }

}
