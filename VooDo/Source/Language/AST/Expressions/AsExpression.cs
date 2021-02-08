

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record AsExpression(Expression Expression, ComplexType Type) : Expression
    {

        #region Overrides

        public override string ToString() => $"{Expression} as {Type}";

        #endregion

    }

}
