using System;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;

namespace VooDo.Compilation
{

    public sealed record Global(ComplexTypeOrVar Type, Identifier? Name, Expression? Initializer = null)
    {

        public bool IsAnonymous => Name is null;
        public bool HasInitializer => Initializer is not null;

    }

    public sealed record GlobalPrototype
    {

        private readonly Node m_syntax;

        public GlobalPrototype(Global _global, DeclarationStatement.Declarator _declarator)
        {
            if (_global.IsAnonymous)
            {
                throw new ArgumentException("Expected named global", nameof(_global));
            }
            Global = _global;
            m_syntax = _declarator;
        }

        public GlobalPrototype(Global _global, GlobalExpression _expression)
        {
            if (_global.IsAnonymous)
            {
                throw new ArgumentException("Expected anonymous global", nameof(_global));
            }
            Global = _global;
            m_syntax = _expression;
        }

        public Global Global { get; }
        public DeclarationStatement.Declarator? Declarator => m_syntax as DeclarationStatement.Declarator;
        public GlobalExpression? Expression => m_syntax as GlobalExpression;

    }

}
