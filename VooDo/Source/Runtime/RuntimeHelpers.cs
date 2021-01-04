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

        private const bool c_accessStaticMembersThroughInstance = false;

        private static BindingFlags GetMemberFlags(bool _includeStatic, bool _includeInstance)
            => (_includeInstance ? BindingFlags.Instance : 0) | (_includeStatic ? BindingFlags.Static : 0) | BindingFlags.Public;

        private static EventInfo GetEvent(Type _type, Name _name, bool _includeStatic, bool _includeInstance)
            => _type.GetEvent(_name, GetMemberFlags(_includeStatic, _includeInstance));

        private static FieldInfo GetField(Type _type, Name _name, bool _includeStatic, bool _includeInstance)
            => _type.GetField(_name, GetMemberFlags(_includeStatic, _includeInstance));

        private static Type GetNestedType(Type _type, Name _name)
            => _type.GetNestedType(_name, GetMemberFlags(false, false));

        private static PropertyInfo GetProperty(Type _type, Name _name, bool _includeStatic, bool _includeInstance)
            => _type.GetProperty(_name, GetMemberFlags(_includeStatic, _includeInstance));

        private static MethodInfo[] GetMethods(Type _type, Name _name, bool _includeStatic, bool _includeInstance)
        {
            IEnumerable<MethodInfo> methods = _type.GetMember(_name, MemberTypes.Method, GetMemberFlags(_includeStatic, _includeInstance)).Cast<MethodInfo>();
            if (GetIndexers(_type).FirstOrDefault()?.Name is string indexerName)
            {
                methods = methods.Where(_m => _m.Name != $"get_{indexerName}" && _m.Name != $"set_{indexerName}");
            }
            return methods.ToArray();
        }

        private static PropertyInfo[] GetIndexers(Type _type)
            => _type.GetProperties(GetMemberFlags(false, true))
                .Cast<PropertyInfo>()
                .Where(_p => _p.GetIndexParameters().Any())
                .ToArray();

        private static MethodInfo GetUserDefinedConversion(Type _type, Type _targetType)
        {

            bool HasRightSignature(MethodInfo _method)
            {
                ParameterInfo[] parameters = _method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.Equals(_type) && !parameters[0].IsOut && _method.ReturnType.Equals(_type);
            }

            return GetMethods(_type, "op_Explicit", true, false)
            .Concat(GetMethods(_type, "op_Implicit", true, false))
            .Where(HasRightSignature)
            .FirstOrDefault();
        }

        private static T ChooseOverload<T>(this IEnumerable<T> _overloads, Func<T, ParameterInfo[]> _mapper, object[] _arguments, Type[] _types) where T : MemberInfo
        {

            bool HasOutParameters(ParameterInfo[] _parameters) => _parameters.Any(_p => _p.IsOut);

            bool DoesOverloadMatch(ParameterInfo[] _parameters)
                => _parameters.Length == _arguments.Length &&
                _parameters.Zip(_arguments, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.IsAssignableFrom(_a.GetType())).All(_t => _t);

            IEnumerable<Type> actualTypes = _arguments.Select(_a => _a.GetType());
            IEnumerable<Type> matchTypes = _types?.Zip(actualTypes, (_t, _a) => _t ?? _a) ?? actualTypes;

            bool DoesOverloadMatchExactly(ParameterInfo[] _parameters)
                => _parameters.Length == _arguments.Length &&
                _parameters.Zip(_types, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.Equals(_a)).All(_x => _x);

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
                matches = matches.Where(_m => matches.All(_mt => _mt.DeclaringType.IsAssignableFrom(_m.DeclaringType))).ToArray();
                if (matches.Length == 1)
                {
                    return matches[0];
                }
                else
                {
                    throw new AmbiguousMatchException();
                }
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
            if (_instance != null && !_type.IsInstanceOfType(_instance))
            {
                throw new ArgumentException("Wrong instance type", nameof(_instance));
            }
            if (_instance is IMemberProvider provider)
            {
                return provider.EvaluateMember(_name, _hookManager);
            }
            bool includeInstance = _instance != null;
            bool includeStatic = _instance == null || c_accessStaticMembersThroughInstance;
            if (GetEvent(_type, _name, includeStatic, includeInstance) is EventInfo eventInfo)
            {
                return eventInfo.Equals(_hookManager.CurrentEvent);
            }
            else if (GetProperty(_type, _name, includeStatic, includeInstance) is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(_instance);
            }
            else if (GetField(_type, _name, includeStatic, includeInstance) is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue(_instance);
            }
            else if (GetNestedType(_type, _name) is Type typeInfo)
            {
                return new TypeWrapper(typeInfo);
            }
            else if (GetMethods(_type, _name, includeStatic, includeInstance) is MethodInfo[] methodInfo)
            {
                return new MethodWrapper(methodInfo, _instance);
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        internal static void AssignMember(Name _name, object _value, Type _type, object _instance = null)
        {
            Ensure.NonNull(_type, nameof(_type));
            Ensure.NonNull(_name, nameof(_name));
            if (_instance != null && !_instance.GetType().IsInstanceOfType(_type))
            {
                throw new ArgumentException("Wrong instance type", nameof(_instance));
            }
            if (_instance is IAssignableMemberProvider provider)
            {
                provider.AssignMember(_name, _value);
            }
            bool includeInstance = _instance != null;
            bool includeStatic = _instance == null || c_accessStaticMembersThroughInstance;
            if (GetProperty(_type, _name, includeStatic, includeInstance) is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(_instance, _value);
            }
            else if (GetField(_type, _name, includeStatic, includeInstance) is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(_instance, _value);
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        internal static object EvaluateIndexer(object _instance, object[] _arguments, Type[] _argumentTypes = null)
        {
            Ensure.NonNull(_instance, nameof(_instance));
            Ensure.NonNull(_arguments, nameof(_arguments));
            PropertyInfo[] properties = GetIndexers(_instance.GetType());
            PropertyInfo property;
            try
            {
                property = ChooseOverload(properties, _p => _p.GetIndexParameters(), _arguments, _argumentTypes);
            }
            catch (Exception)
            {
                if (_instance is Array array)
                {
                    return array.GetValue(_arguments.Select(_a => Convert.ToInt64(_a)).ToArray());
                }
                else
                {
                    throw;
                }
            }
            return property.GetValue(_instance, _arguments);
        }

        internal static void AssignIndexer(object _instance, object[] _arguments, object _value, Type[] _argumentTypes = null)
        {
            Ensure.NonNull(_instance, nameof(_instance));
            Ensure.NonNull(_arguments, nameof(_arguments));
            PropertyInfo[] properties = GetIndexers(_instance.GetType());
            PropertyInfo property;
            try
            {
                property = ChooseOverload(properties, _p => _p.GetIndexParameters(), _arguments, _argumentTypes);
            }
            catch (Exception)
            {
                if (_instance is Array array)
                {
                    array.SetValue(_value, _arguments.Select(_a => Convert.ToInt64(_a)).ToArray());
                    return;
                }
                else
                {
                    throw;
                }
            }
            property.SetValue(_instance, _arguments, _arguments);
        }

        internal static object InvokeMethod(IEnumerable<MethodInfo> _methodGroup, object[] _arguments, object _instance = null, Type[] _argumentTypes = null)
        {
            Ensure.NonNull(_methodGroup, nameof(_methodGroup));
            Ensure.NonNull(_arguments, nameof(_arguments));
            MethodInfo method = ChooseOverload(_methodGroup, _m => _m.GetParameters(), _arguments, _argumentTypes);
            return method.Invoke(_instance, _arguments);
        }

        internal static object ConvertType(object _source, Type _target)
        {
            Ensure.NonNull(_target, nameof(_target));
            try
            {
                return GetUserDefinedConversion(_source.GetType(), _target).Invoke(null, new object[] { _source });
            }
            catch
            { }
            try
            {
                return Convert.ChangeType(_source, _target);
            }
            catch
            { }
            throw new InvalidCastException();
        }

        internal static T Cast<T>(dynamic _source)
        {
            try
            {
                return (T) _source;
            }
            catch
            {
                throw new InvalidCastException();
            }
        }

    }

}
