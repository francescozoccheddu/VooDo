using System.Collections.Generic;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ConditionalExpression(Expression Condition, Expression True, Expression False) : Expression
    {

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Condition, True, False };
        public override string ToString() => $"{Condition} ? {True} : {False}";

        #endregion

    }

}
