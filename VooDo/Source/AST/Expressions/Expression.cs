using System;

namespace VooDo.AST.Expressions
{

    public abstract record Expression : ComplexTypeOrExpression
    {

        /*
            {
                SimpleAssignmentExpression:
                AddAssignmentExpression:
                SubtractAssignmentExpression:
                MultiplyAssignmentExpression:
                DivideAssignmentExpression:
                ModuloAssignmentExpression:
                AndAssignmentExpression:
                ExclusiveOrAssignmentExpression:
                OrAssignmentExpression:
                LeftShiftAssignmentExpression:
                RightShiftAssignmentExpression:
                CoalesceAssignmentExpression:
                    Assignment;
                CoalesceExpression:
                    Coalescing;
                LogicalOrExpression:
                    ConditionalOr;
                LogicalAndExpression:
                    ConditionalAnd;
                BitwiseOrExpression:
                    LogicalOr;
                ExclusiveOrExpression:
                    LogicalXor;
                BitwiseAndExpression:
                    LogicalAnd;
                EqualsExpression:
                NotEqualsExpression:
                    Equality;
                LessThanExpression:
                LessThanOrEqualExpression:
                GreaterThanExpression:
                GreaterThanOrEqualExpression:
                IsExpression:
                AsExpression:
                IsPatternExpression:
                    Relational;
                LeftShiftExpression:
                RightShiftExpression:
                    Shift;
                AddExpression:
                SubtractExpression:
                    Additive;
                MultiplyExpression:
                DivideExpression:
                ModuloExpression:
                    Mutiplicative;
                UnaryPlusExpression:
                UnaryMinusExpression:
                BitwiseNotExpression:
                LogicalNotExpression:
                PreIncrementExpression:
                PreDecrementExpression:
                    Unary;
                CastExpression:
                    Cast;
                ConditionalExpression:
                    Expression;
                default:
                    Primary;
        */

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

        public abstract override Expression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

    }

}
