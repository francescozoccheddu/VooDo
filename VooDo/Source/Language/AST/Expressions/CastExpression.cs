using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record CastExpression(ComplexType Type, Expression Expression) : Expression
    {

        #region Overrides

        public override string ToString() => $"({Type}) {Expression}";

        #endregion

    }

}
