﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record AssignmentStatement(AssignableExpression Target, AssignmentStatement.EKind Kind, Expression Source) : SingleStatement
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

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
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
                    },
                    (ExpressionSyntax) Target.EmitNode(_scope, _tagger),
                    (ExpressionSyntax) Source.EmitNode(_scope, _tagger)))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => new[] { Target, Source };
        public override string ToString() => $"{Target} {Kind.Token()} {Source}{GrammarConstants.statementEndToken}";

        #endregion

    }


}

