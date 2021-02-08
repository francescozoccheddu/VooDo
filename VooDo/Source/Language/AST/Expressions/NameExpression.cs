using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record NameExpression(bool IsControllerOf, Identifier Name) : AssignableExpression
    {

        #region Overrides

        public override string ToString() => (IsControllerOf ? "$" : "") + $"{Name}";

        #endregion

    }

}
