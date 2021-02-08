

using System.Collections.Generic;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record AsExpression(Expression Expression, ComplexType Type) : Expression
    {

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Expression, Type };
        public override string ToString() => $"{Expression} {GrammarConstants.asKeyword} {Type}";

        #endregion

    }

}
