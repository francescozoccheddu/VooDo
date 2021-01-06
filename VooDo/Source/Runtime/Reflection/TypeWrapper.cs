using System;

using VooDo.AST;
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

        void IAssignableMemberProvider.AssignMember(Env _env, Name _name, Eval _value)
            => Utils.Reflection.AssignMember(_env, new Eval(null, Type), _name, _value);

        Eval IMemberProvider.EvaluateMember(Env _env, Name _name)
            => Utils.Reflection.EvaluateMember(_env, new Eval(null, Type), _name);

        Eval IGeneric.Specialize(Env _env, Type[] _arguments)
        {
            if (Type.IsGenericTypeDefinition)
            {
                return new Eval(new TypeWrapper(Type.MakeGenericType(_arguments)));
            }
            else
            {
                throw new Exception("Not a generic type definition");
            }
        }

    }
}
