﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Language.AST.Expressions
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

        public LiteralExpression()
        {
            Value = null;
        }

        private LiteralExpression(object? _value = null)
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
                    throw new ArgumentException("Not a literal type");
                }
                m_value = value;
            }
        }

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => Enumerable.Empty<Node>();
        public override string ToString() => Value switch
        {
            string => $"\"{Value}\"",
            char => $"'{Value}'",
            null => "null",
            _ => $"{Value}"
        };

        #endregion

    }
}
