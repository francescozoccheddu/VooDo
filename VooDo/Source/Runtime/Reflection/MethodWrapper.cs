using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.Runtime.Meta;
using VooDo.Utils;

namespace VooDo.Runtime.Reflection
{

    public sealed class MethodWrapper : ICallable, IGeneric
    {

        public static MethodWrapper FromStaticMethodGroup(Type _declaringType, Name _name)
        {
            Ensure.NonNull(_declaringType, nameof(_declaringType));
            Ensure.NonNull(_name, nameof(_name));
            return new MethodWrapper(_declaringType.GetMember(_name).Cast<MethodInfo>());
        }

        public static MethodWrapper FromMethodGroup(object _instance, Name _name)
        {
            Ensure.NonNull(_instance, nameof(_instance));
            Ensure.NonNull(_name, nameof(_name));
            return new MethodWrapper(_instance.GetType().GetMember(_name).Cast<MethodInfo>(), _instance);
        }

        public MethodWrapper(MethodInfo _methodInfo, object _instance = null) : this(new MethodInfo[] { _methodInfo }, _instance)
        { }

        public MethodWrapper(IEnumerable<MethodInfo> _methodInfos, object _instance = null)
        {
            Ensure.NonNull(_methodInfos, nameof(_methodInfos));
            Methods = _methodInfos.ToList().AsReadOnly();
            Ensure.NonNullItems(Methods, nameof(_methodInfos));
            if (Methods.Count == 0)
            {
                throw new ArgumentException("Empty group", nameof(_methodInfos));
            }
            if (Methods.Any(_m => _m.MemberType != MemberTypes.Method))
            {
                throw new ArgumentException("Non-method member type", nameof(_methodInfos));
            }
            if (Methods.Any(_m => _m.DeclaringType != DeclaringType || _m.Name != Name || !_m.GetGenericArguments().SequenceEqual(GenericArguments)))
            {
                throw new ArgumentException("Not a method group", nameof(_methodInfos));
            }
            Instance = _instance;
        }

        public object Instance { get; }
        public IReadOnlyList<MethodInfo> Methods { get; }
        public string Name => Methods[0].Name;
        public Type DeclaringType => Methods[0].DeclaringType;
        public IReadOnlyList<Type> GenericArguments => Methods[0].GetGenericArguments();
        public bool IsBound => Instance != null;

        public MethodWrapper Bind(object _instance) => new MethodWrapper(Methods, _instance);

        public sealed override bool Equals(object _obj)
            => _obj is MethodWrapper wrapper && Methods.Count == wrapper.Methods.Count && Methods.All(wrapper.Methods.Contains);

        public sealed override int GetHashCode() => Identity.CombineHash(Instance, Identity.CombineHashes(Methods));

        public override string ToString()
            => $"{Methods[0].DeclaringType}.{Methods[0].Name}{(GenericArguments.Count > 0 ? $"<{string.Join(", ", GenericArguments)}>" : "")}";

        Eval ICallable.Call(Env _env, Eval[] _arguments) => Call(_arguments);

        public Eval Call(Eval[] _arguments)
        {
            Ensure.NonNull(_arguments, nameof(_arguments));
            return Utils.Reflection.InvokeMethod(Methods, Instance, _arguments);
        }

        public MethodWrapper Specialize(Type[] _arguments)
        {
            MethodInfo GetSpecialized(MethodInfo _method)
            {
                try
                {
                    return _method.MakeGenericMethod(_arguments);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return new MethodWrapper(Methods.Select(_m => GetSpecialized(_m)).Where(_m => _m != null), Instance);
        }

        Eval IGeneric.Specialize(Env _env, Type[] _arguments) => new Eval(Specialize(_arguments));

    }

}
