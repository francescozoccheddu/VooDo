using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Errors.Problems;

namespace VooDo.AST.Expressions
{

    public sealed record LiteralExpression : Expression
    {

        #region Creation

        public static LiteralExpression Null { get; } = new LiteralExpression((object?) null);
        public static LiteralExpression True { get; } = new LiteralExpression(true);
        public static LiteralExpression False { get; } = new LiteralExpression(false);

        public static LiteralExpression Create(bool _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(int _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(uint _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(short _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(ushort _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(long _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(ulong _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(decimal _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(string _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(char _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(sbyte _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(byte _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(float _value)
            => new LiteralExpression(_value);

        public static LiteralExpression Create(double _value)
            => new LiteralExpression(_value);

        #endregion

        #region Members

        public LiteralExpression(object? _value = null)
        {
            Value = _value;
        }

        private object? m_value;
        public object? Value
        {
            get => m_value;
            init
            {
                if (value is not (null
                    or bool
                    or int
                    or uint
                    or short
                    or ushort
                    or long
                    or ulong
                    or decimal
                    or sbyte
                    or byte
                    or char
                    or string
                    or float
                    or double))
                {
                    throw new SyntaxError(this, "Non literal value type").AsThrowable();
                }
                m_value = value;
            }
        }

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override LiteralExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map) => this;

        private LiteralExpressionSyntax EmitNode()
        {
            SyntaxKind kind = m_value switch
            {
                true => SyntaxKind.TrueLiteralExpression,
                false => SyntaxKind.FalseLiteralExpression,
                null => SyntaxKind.NullLiteralExpression,
                char => SyntaxKind.CharacterLiteralExpression,
                string => SyntaxKind.StringLiteralExpression,
                _ => SyntaxKind.NumericLiteralExpression
            };
            return m_value is bool or null
                ? SyntaxFactory.LiteralExpression(kind)
                : SyntaxFactory.LiteralExpression(kind, m_value switch
                {
                    char v => SyntaxFactory.Literal(v),
                    decimal v => SyntaxFactory.Literal(v),
                    string v => SyntaxFactory.Literal(v),
                    uint v => SyntaxFactory.Literal(v),
                    double v => SyntaxFactory.Literal(v),
                    float v => SyntaxFactory.Literal(v),
                    ulong v => SyntaxFactory.Literal(v),
                    long v => SyntaxFactory.Literal(v),
                    int v => SyntaxFactory.Literal(v),
                    _ => throw new InvalidOperationException()
                });
        }
        internal override LiteralExpressionSyntax EmitNode(Scope _scope, Tagger _tagger) => EmitNode().Own(_tagger, this);
        public override string ToString() => EmitNode().ToFullString();

        #endregion

    }
}
