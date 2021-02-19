using System;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;

namespace VooDo.Compiling.Emission
{

    public sealed record Global(bool IsConstant, ComplexTypeOrVar Type, Identifier? Name, Expression? Initializer = null)
    {

        public bool IsAnonymous => Name is null;
        public bool HasInitializer => Initializer is not null;

    }

    public sealed record GlobalPrototype
    {

        public GlobalPrototype(Global _global, DeclarationStatement.Declarator _declarator)
        {
            if (_global.IsAnonymous)
            {
                throw new ArgumentException("Expected named global", nameof(_global));
            }
            Global = _global;
            Declarator = _declarator;
        }

        public GlobalPrototype(Global _global, GlobalExpression _expression)
        {
            if (!_global.IsAnonymous)
            {
                throw new ArgumentException("Expected anonymous global", nameof(_global));
            }
            Global = _global;
            Expression = _expression;
        }

        public Global Global { get; }
        public DeclarationStatement.Declarator? Declarator { get; }
        public GlobalExpression? Expression { get; }
        public Node Source => Expression ?? (Node) Declarator!;

    }

}
