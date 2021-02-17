namespace VooDo.AST.Expressions
{

    public abstract record Expression : ComplexTypeOrExpression
    {

        protected enum EPrecedence
        {
            Global, Coalesce, LogicOr, LogicAnd, BitwiseOr, BitwiseXor, BitwiseAnd, Equality, Relational, Shift, Additive, Multiplicative, Unary, Cast, Conditional, Primary
        }

        protected abstract EPrecedence m_Precedence { get; }

        protected string LeftCode(ComplexTypeOrExpression _expression)
            => LeftCode(_expression, m_Precedence);

        protected string RightCode(ComplexTypeOrExpression _expression)
            => RightCode(_expression, m_Precedence);

        protected static string LeftCode(ComplexTypeOrExpression _expression, EPrecedence _currentPrecedence)
        {
            if (_expression is Expression expression && expression.m_Precedence < _currentPrecedence)
            {
                return $"({expression})";
            }
            else
            {
                return _expression.ToString();
            }
        }

        protected static string RightCode(ComplexTypeOrExpression _expression, EPrecedence _currentPrecedence)
        {
            if (_expression is Expression expression && expression.m_Precedence <= _currentPrecedence)
            {
                return $"({expression})";
            }
            else
            {
                return _expression.ToString();
            }
        }

    }

}
