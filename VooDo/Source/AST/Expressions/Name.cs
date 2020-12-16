
using System;

using VooDo.Utils;

namespace VooDo.AST
{

    public sealed class Name : ASTBase
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

        #region ASTBase

        public sealed override string Code => m_name;

        public sealed override bool Equals(object _obj) => _obj is Name other && other.m_name == m_name;
        public sealed override int GetHashCode() => m_name.GetHashCode();

        #endregion

    }

}
