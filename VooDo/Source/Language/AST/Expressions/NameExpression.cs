using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record NameExpression(Identifier Name) : AssignableExpression
    {

        #region Overrides

        public override string ToString() => $"{Name}";

        #endregion

    }

}
