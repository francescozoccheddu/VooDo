

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, Identifier? Name = null) : Expression
    {

        #region Members

        public bool IsDeclaration => Name is not null;

        #endregion

        #region Overrides

        public override string ToString() => $"{Expression} is {Type}" + (IsDeclaration ? $" {Name}" : "");

        #endregion

    }

}
