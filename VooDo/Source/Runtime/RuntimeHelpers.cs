using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.Runtime.Meta;
using VooDo.Runtime.Reflection;
using VooDo.Utils;

namespace VooDo.Runtime.Engine
{

    internal static class RuntimeHelpers
    {

        private static MemberInfo[] GetMembers(Type _type, Name _name)
        {
            MemberInfo[] members = _type.GetMember(_name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (members.Length == 0)
            {
                throw new MissingMemberException();
            }
            MemberInfo member = members[0];
            MemberTypes memberType = member.MemberType;
            if (members.Any(_m => _m.MemberType != memberType))
            {
                throw new AmbiguousMatchException("Ambiguous member types");
            }
            if (members.Length > 1 && memberType != MemberTypes.Method)
            {
                throw new AmbiguousMatchException("Ambiguous non-method members");
            }
            return members;
        }

        private static PropertyInfo[] GetIndexer(Type _type) => _type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(_p => _p.GetIndexParameters().Any()).ToArray();

        private static T ChooseOverload<T>(this IEnumerable<T> _overloads, Func<T, ParameterInfo[]> _mapper, object[] _arguments)
        {

            bool HasOutParameters(ParameterInfo[] _parameters) => _parameters.Any(_p => _p.IsOut);

            bool DoesOverloadMatch(ParameterInfo[] _parameters)
                => _parameters.Length == _arguments.Length &&
                _parameters.Zip(_arguments, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.IsAssignableFrom(_a.GetType())).All(_t => _t);


            bool DoesOverloadMatchExactly(ParameterInfo[] _parameters)
                => _parameters.Length == _arguments.Length &&
                _parameters.Zip(_arguments, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.Equals(_a.GetType())).All(_t => _t);


            T[] matches = _overloads.Where(_o => !HasOutParameters(_mapper(_o)) && DoesOverloadMatch(_mapper(_o))).ToArray();
            if (matches.Length == 1)
            {
                return matches[0];
            }
            matches = _overloads.Where(_o => DoesOverloadMatchExactly(_mapper(_o))).ToArray();
            if (matches.Length == 1)
            {
                return matches[0];
            }
            else if (matches.Length > 0)
            {
                throw new AmbiguousMatchException();
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        internal static object EvaluateMember(HookManager _hookManager, Name _name, Type _type, object _instance = null)
        {
            Ensure.NonNull(_type, nameof(_type));
            Ensure.NonNull(_name, nameof(_name));
            Ensure.NonNull(_hookManager, nameof(_hookManager));
            if (_instance != null && !_instance.GetType().Equals(_type))
            {
                throw new ArgumentException("Wrong instance type", nameof(_instance));
            }
            if (_instance is IMemberProvider provider)
            {
                return provider.EvaluateMember(_name, _hookManager);
            }
            MemberInfo[] members = GetMembers(_type, _name);
            MemberInfo member = members[0];
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                return ((EventInfo) member).Equals(_hookManager.CurrentEvent);
                case MemberTypes.Property:
                return ((PropertyInfo) member).GetValue(_instance);
                case MemberTypes.Field:
                return ((FieldInfo) member).GetValue(_instance);
                case MemberTypes.Method:
                return new MethodWrapper(members.Cast<MethodInfo>(), _instance);
                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                return new TypeWrapper((Type) member);
                default:
                throw new MemberAccessException();
            }
        }

        internal static void AssignMember(Name _name, object _value, Type _type, object _instance = null)
        {
            Ensure.NonNull(_type, nameof(_type));
            Ensure.NonNull(_name, nameof(_name));
            if (_instance != null && !_instance.GetType().Equals(_type))
            {
                throw new ArgumentException("Wrong instance type", nameof(_instance));
            }
            if (_instance is IAssignableMemberProvider provider)
            {
                provider.AssignMember(_name, _value);
            }
            MemberInfo[] members = GetMembers(_type, _name);
            MemberInfo member = members[0];
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                ((PropertyInfo) member).SetValue(_instance, _value);
                break;
                case MemberTypes.Field:
                ((FieldInfo) member).SetValue(_instance, _value);
                break;
                default:
                throw new MemberAccessException();
            }
        }

        internal static object EvaluateIndexer(object _instance, object[] _arguments)
        {
            Ensure.NonNull(_instance, nameof(_instance));
            Ensure.NonNull(_arguments, nameof(_arguments));
            PropertyInfo[] properties = GetIndexer(_instance.GetType());
            PropertyInfo property = ChooseOverload(properties, _p => _p.GetIndexParameters(), _arguments);
            return property.GetValue(_instance, _arguments);
        }

        internal static void AssignIndexer(object _instance, object[] _arguments, object _value)
        {
            Ensure.NonNull(_instance, nameof(_instance));
            Ensure.NonNull(_arguments, nameof(_arguments));
            PropertyInfo[] properties = GetIndexer(_instance.GetType());
            PropertyInfo property = ChooseOverload(properties, _p => _p.GetIndexParameters(), _arguments);
            property.SetValue(_instance, _value, _arguments);
        }

        internal static object InvokeMethod(IEnumerable<MethodInfo> _methodGroup, object[] _arguments, object _instance = null)
        {
            Ensure.NonNull(_methodGroup, nameof(_methodGroup));
            Ensure.NonNull(_arguments, nameof(_arguments));
            MethodInfo method = ChooseOverload(_methodGroup, _m => _m.GetParameters(), _arguments);
            return method.Invoke(_instance, _arguments);
        }

        internal static bool TryCast<T>(object _source, out T _target)
        {
            if (_source is T target)
            {
                _target = target;
                return true;
            }
            else
            {
                _target = default;
                return false;
            }
        }

        internal static T Cast<T>(object _source)
        {
            if (TryCast<T>(_source, out T target))
            {
                return target;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

    }

}
