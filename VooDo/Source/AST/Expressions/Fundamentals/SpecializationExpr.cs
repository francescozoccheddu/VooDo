﻿using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Runtime.Meta;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class SpecializationExpr : ParametricExpr
    {

        internal SpecializationExpr(Expr _callable, IEnumerable<Expr> _arguments) : base(_callable, _arguments)
        {
        }

        #region Expr

        internal sealed override Eval Evaluate(Env _env)
        {
            Eval source = Source.Evaluate(_env);
            if (source.Value is IGeneric generic)
            {
                return generic.Specialize(_env, Arguments.Select(_a => _a.AsType(_env)).ToArray());
            }
            else
            {
                throw new Exception("Source Expr does not evaluate to an IGeneric object");
            }
        }

        public sealed override int Precedence => 0;

        public sealed override string Code => $"{Source.LeftCode(Precedence)}<{Arguments.ArgumentsListCode()}>";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj) => _obj is SpecializationExpr && base.Equals(_obj);

        public override int GetHashCode() => base.GetHashCode();

        #endregion

    }

}
