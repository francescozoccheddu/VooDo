﻿using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime.Engine;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class IndexExpr : ParametricExpr
    {

        internal IndexExpr(Expr _indexable, IEnumerable<Expr> _index, bool _nullCoalesce) : base(_indexable, _index)
        {
            if (Arguments.Count == 0)
            {
                throw new ArgumentException("Empty index", nameof(_index));
            }
            NullCoalesce = _nullCoalesce;
        }

        public bool NullCoalesce { get; }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env)
            => RuntimeHelpers.EvaluateIndexer(Source.Evaluate(_env), Arguments.Select(_a => _a.Evaluate(_env)).ToArray());

        internal sealed override void Assign(Runtime.Env _env, object _value)
            => RuntimeHelpers.AssignIndexer(Source.Evaluate(_env), Arguments.Select(_a => _a.Evaluate(_env)).ToArray(), _value);

        public sealed override int Precedence => 0;

        public sealed override string Code
            => $"{Source.LeftCode(Precedence)}{(NullCoalesce ? "?" : "")}[{Arguments.ArgumentsListCode()}]";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is IndexExpr expr && NullCoalesce == expr.NullCoalesce && base.Equals(expr);

        public sealed override int GetHashCode()
            => Identity.CombineHash(base.GetHashCode(), NullCoalesce);

        #endregion

    }

}