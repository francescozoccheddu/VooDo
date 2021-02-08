using System.Collections.Generic;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record CastExpression(ComplexType Type, Expression Expression) : Expression
    {

        #region Overrides


        public override IEnumerable<Node> Children => new Node[] { Expression, Type };
        public override string ToString() => $"({Type}) {Expression}";

        #endregion

    }

}
