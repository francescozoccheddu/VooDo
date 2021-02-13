using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Statements
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

        public override AssignmentStatement ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            AssignableExpression newTarget = (AssignableExpression) _map(Target).NonNull();
            Expression newExpression = (Expression) _map(Source).NonNull();
            if (ReferenceEquals(newTarget, Target) && ReferenceEquals(newExpression, Source))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Target = newTarget,
                    Source = newExpression
                };
            }
        }

        internal override StatementSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    Kind switch
                    {
                        EKind.Simple => SyntaxKind.SimpleAssignmentExpression,
                        EKind.Add => SyntaxKind.AddAssignmentExpression,
                        EKind.Subtract => SyntaxKind.SubtractAssignmentExpression,
                        EKind.Multiply => SyntaxKind.MultiplyAssignmentExpression,
                        EKind.Divide => SyntaxKind.DivideAssignmentExpression,
                        EKind.Modulo => SyntaxKind.ModuloAssignmentExpression,
                        EKind.LeftShift => SyntaxKind.LeftShiftAssignmentExpression,
                        EKind.RightShift => SyntaxKind.RightShiftAssignmentExpression,
                        EKind.BitwiseAnd => SyntaxKind.AndAssignmentExpression,
                        EKind.BitwiseOr => SyntaxKind.OrAssignmentExpression,
                        EKind.BitwiseXor => SyntaxKind.ExclusiveOrAssignmentExpression,
                        EKind.Coalesce => SyntaxKind.CoalesceAssignmentExpression,
                        _ => throw new InvalidOperationException(),
                    },
                    Target.EmitNode(_scope, _marker),
                    Source.EmitNode(_scope, _marker)))
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Target, Source };
        public override string ToString() => $"{Target} {Kind.Token()} {Source}{GrammarConstants.statementEndToken}";

        #endregion

    }


}

