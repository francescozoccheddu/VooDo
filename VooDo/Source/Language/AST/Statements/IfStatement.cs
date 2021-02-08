using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null) : Statement
    {

        #region Members

        public bool HasElse => Else is not null;

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Condition, Then }.Concat(HasElse ? new[] { Else! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{GrammarConstants.ifKeyword} ({Condition})\n"
            + (Then is BlockStatement ? "" : "\t") + Then
            + (Else is null ? "" : $"\n{GrammarConstants.elseKeyword}\n" + (Else is BlockStatement ? "" : "\t") + Else);

        #endregion

    }

}
