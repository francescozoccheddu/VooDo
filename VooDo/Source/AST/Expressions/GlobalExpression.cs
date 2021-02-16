﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record GlobalExpression : Expression
    {

        #region Members

        public GlobalExpression(Expression _controller, Expression? _initializer = null)
        {
            Controller = _controller;
            Initializer = _initializer;
        }

        public Expression Controller { get; init; }

        private Expression? m_initializer;
        public Expression? Initializer
        {
            get => m_initializer;
            init
            {
                if (value is not null)
                {
                    GlobalExpression? child = value.DescendantNodes().OfType<GlobalExpression>().SingleOrDefault();
                    if (child is not null)
                    {
                        throw new ChildSyntaxError(this, child, "Global expression initializer cannot contain global expressions").AsThrowable();
                    }
                }
                m_initializer = value;
            }
        }

        public bool HasInitializer => Initializer is not null;

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Global;

        public override GlobalExpression ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newController = (Expression) _map(Controller).NonNull();
            Expression? newInitializer = (Expression?) _map(Initializer);
            if (ReferenceEquals(newController, Controller) && ReferenceEquals(newInitializer, Initializer))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Controller = newController,
                    Initializer = newInitializer
                };
            }
        }

        internal override InvocationExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
        {
            Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new GlobalPrototype(new Global(ComplexTypeOrVar.Var, null, Initializer), this));
            return SyntaxFactoryUtils.SetControllerAndGetValueInvocation(
                    SyntaxFactoryUtils.ThisMemberAccess(globalDefinition.Identifier),
                    Controller.EmitNode(_scope, _tagger).Own(_tagger, Controller))
                .Own(_tagger, this);
        }
        public override IEnumerable<Expression> Children => HasInitializer ? new Expression[] { Controller, Initializer! } : new Expression[] { Controller };
        public override string ToString() => $"{GrammarConstants.globKeyword} {Controller}" + (HasInitializer ? $" {GrammarConstants.initKeyword} {Initializer}" : "");

        #endregion

    }

}