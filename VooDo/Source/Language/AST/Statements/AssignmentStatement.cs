using System.Collections.Generic;

using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record AssignmentStatement(AssignableExpression Target, AssignmentStatement.EKind Kind, Expression Source) : Statement
    {

        #region Nested types

        public enum EKind
        {
            Simple,
            Add, Subtract,
            Multiply, Divide, Modulo,
            LeftShift, RightShift,
            BitwiseAnd, BitwiseOr, BitwiseXor,
            Coalesce
        }

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Target, Source };
        public override string ToString() => $"{Target} {Kind.Token()} {Source}{GrammarConstants.statementEndToken}";

        #endregion

    }


}

