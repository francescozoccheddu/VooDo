

using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, Identifier? Name = null) : Expression
    {

        #region Members

        public bool IsDeclaration => Name is not null;

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Expression, Type }.Concat(IsDeclaration ? new Node[] { Name! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{Expression} {GrammarConstants.isKeyword} {Type}" + (IsDeclaration ? $" {Name}" : "");

        #endregion

    }

}
