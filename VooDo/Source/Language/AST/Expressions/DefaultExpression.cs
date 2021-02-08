using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record DefaultExpression(ComplexType? Type = null) : Expression
    {

        #region Members

        public bool HasType => Type is not null;

        #endregion

        #region Overrides

        public override string ToString() => GrammarConstants.defaultKeyword + (HasType ? $"({Type})" : "");

        #endregion

    }

}
