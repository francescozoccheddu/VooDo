using System;

using VooDo.AST;
using VooDo.Runtime.Engine;
using VooDo.Runtime.Meta;

namespace VooDo.Runtime.Reflection
{
    public sealed class TypeWrapper : IAssignableMemberProvider, IGeneric
    {

        public TypeWrapper(Type _type) => Type = _type;

        public Type Type { get; }

        public static implicit operator Type(TypeWrapper _wrapper) => _wrapper.Type;

        public override bool Equals(object _obj) => _obj is TypeWrapper wrapper && Type.Equals(wrapper.Type);

        public override int GetHashCode() => Type.GetHashCode();

        public override string ToString() => Type.ToString();

        void IAssignableMemberProvider.AssignMember(Name _name, object _value)
            => RuntimeHelpers.AssignMember(_name, _value, Type, null);

        object IMemberProvider.EvaluateMember(Name _name, HookManager _hookManager)
            => RuntimeHelpers.EvaluateMember(_hookManager, _name, Type, null);

        object IGeneric.Specialize(Type[] _arguments)
        {
            if (Type.IsGenericTypeDefinition)
            {
                return new TypeWrapper(Type.MakeGenericType(_arguments));
            }
            else
            {
                throw new Exception("Not a generic type definition");
            }
        }

    }
}
