using System.Collections.Generic;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record NameExpression(bool IsControllerOf, Identifier Name) : AssignableExpression
    {

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Name };
        public override string ToString() => (IsControllerOf ? "$" : "") + $"{Name}";

        #endregion

    }

}
