using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null)
    {

        #region Overrides

        public override string ToString() => $"{GrammarConstants.ifKeyword} ({Condition})\n"
            + (Then is BlockStatement ? "" : "\t") + Then
            + (Else is null ? "" : $"\n{GrammarConstants.elseKeyword}\n" + (Else is BlockStatement ? "" : "\t") + Else);

        #endregion

    }

}
