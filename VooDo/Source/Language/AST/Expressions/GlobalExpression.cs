﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record GlobalExpression(Expression Controller, Expression? Initializer) : Expression
    {

        #region Members

        private static bool IsValidInitializer(Expression _expression)
        {
            Stack<BodyNodeOrIdentifier> stack = new Stack<BodyNodeOrIdentifier>();
            stack.Push(_expression);
            while (stack.Count > 0)
            {
                BodyNodeOrIdentifier node = stack.Pop();
                if (node is GlobalExpression)
                {
                    return false;
                }
                foreach (BodyNodeOrIdentifier child in node.Children)
                {
                    stack.Push(child);
                }
            }
            return true;
        }

        private Expression? m_initializer = Initializer?.Assert(IsValidInitializer);
        public Expression? Initializer
        {
            get => m_initializer;
            init => m_initializer = value?.Assert(IsValidInitializer);
        }

        public bool HasInitializer => Initializer is not null;

        #endregion

        #region Overrides

        internal override InvocationExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new Global(ComplexTypeOrVar.Var, null, Initializer));
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName("SetControllerAndGetValue")),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        new ArgumentSyntax[]
                        {
                            SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName(globalDefinition.Identifier))),
                            SyntaxFactory.Argument(Controller.EmitNode(_scope, _marker)).Own(_marker, Controller)
                        })))
                .Own(_marker, this);
        }
        public override IEnumerable<Expression> Children => HasInitializer ? new Expression[] { Controller, Initializer! } : new Expression[] { Controller };
        public override string ToString() => $"{GrammarConstants.globKeyword} {Controller}" + (HasInitializer ? $" {GrammarConstants.initKeyword} {Initializer}" : "");

        #endregion


    }

}
