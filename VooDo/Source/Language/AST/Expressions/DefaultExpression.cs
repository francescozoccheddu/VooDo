using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record DefaultExpression(ComplexType? Type = null) : Expression
    {

        #region Members

        public bool HasType => Type is not null;

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => HasType ? new Node[] { Type! } : Enumerable.Empty<Node>();
        public override string ToString() => GrammarConstants.defaultKeyword + (HasType ? $"({Type})" : "");

        #endregion

    }

}
