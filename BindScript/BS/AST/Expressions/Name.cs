﻿
using BS.Exceptions;

using System;

namespace BS.AST
{

    public sealed class Name
    {

        public static implicit operator Name(string _name)
        {
            return new Name(_name);
        }

        public static implicit operator string(Name _name) => _name.m_name;

        internal Name(string _name)
        {
            Ensure.NonNull(_name, nameof(_name));
            m_name = _name;
        }

        private readonly string m_name;

        public override bool Equals(object _obj) => _obj is string other && other == m_name;
        public override int GetHashCode() => m_name.GetHashCode();
        public override string ToString() => m_name;

    }

}
