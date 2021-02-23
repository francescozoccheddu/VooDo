
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record GlobalExpression : Expression
    {

        
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

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Global;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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


        public override IEnumerable<Node> Children => HasInitializer ? new Expression[] { Controller, Initializer! } : new Expression[] { Controller };
        public override string ToString() => $"{GrammarConstants.globKeyword} {Controller}" + (HasInitializer ? $" {GrammarConstants.initKeyword} {Initializer}" : "");

        
    }

}
