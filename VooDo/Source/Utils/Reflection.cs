using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.Runtime;
using VooDo.Runtime.Meta;
using VooDo.Runtime.Reflection;

namespace VooDo.Utils
{

    internal static class Reflection
    {

        private const bool c_accessStaticMembersByInstance = false;
        private const bool c_accessInstanceMembersByType = true;

        #region Utils

        private static Type[] GetTypes(this IEnumerable<Eval> _evals)
            => _evals.Select(_e => _e.Type).ToArray();

        private static object[] GetValues(this IEnumerable<Eval> _evals)
            => _evals.Select(_e => _e.Value).ToArray();

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

        private static MethodInfo[] GetMethods(Type _type, Name _name, bool _includeStatic, bool _includeInstance, bool _flattenHierarchy = false)
        {
            BindingFlags flags = GetMemberFlags(_includeStatic, _includeInstance) | (_flattenHierarchy ? BindingFlags.FlattenHierarchy : 0);
            return _type.GetMember(_name, MemberTypes.Method, flags).Cast<MethodInfo>().ToArray();
        }

        private static Name GetIndexerName(Type _type)
            => _type.GetProperties(GetMemberFlags(false, true))
                .Cast<PropertyInfo>()
                .First(_p => _p.GetIndexParameters().Any())
                .Name;

        private static MethodInfo GetOperator(Type _type, Name _name, Type[] _types)
            => ChooseOverload(GetMethods(_type, _name, true, false, true), _types);

        private static MethodInfo GetMethod(Type _type, Name _name, Type[] _types)
            => ChooseOverload(GetMethods(_type, _name, true, false, false), _types);

        private static MethodInfo GetIndexerSetter(Type _type, Type[] _types, out Name _indexer)
            => ChooseOverload(GetMethods(_type, $"set_{_indexer = GetIndexerName(_type)}", false, true, false), _types);

        private static MethodInfo GetIndexerGetter(Type _type, Type[] _types, out Name _indexer)
            => ChooseOverload(GetMethods(_type, $"get_{_indexer = GetIndexerName(_type)}", false, true, false), _types);

        private static object[] FillOptionalArguments(ParameterInfo[] _parameters, object[] _arguments)
            => _arguments.Concat(_parameters.Skip(_arguments.Length).Select(_a => _a.DefaultValue)).ToArray();

        private static MethodInfo ChooseOverload(MethodInfo[] _methods, Type[] _types)
        {

            int GetMandatoryParametersCount(ParameterInfo[] _parameters)
                => _parameters.TakeWhile(_p => !_p.IsOptional).Count();

            bool MatchParameters(ParameterInfo[] _parameters, int _argumentsCount, out IEnumerable<ParameterInfo> _match)
            {
                if (_parameters.Length >= _argumentsCount)
                {
                    int mandatory = GetMandatoryParametersCount(_parameters);
                    if (mandatory <= _argumentsCount)
                    {
                        _match = _parameters.Take(_argumentsCount);
                        return true;
                    }
                }
                _match = null;
                return false;
            }

            bool HasOutParameters(IEnumerable<ParameterInfo> _parameters) => _parameters.Any(_p => _p.IsOut);

            bool DoesOverloadMatch(ParameterInfo[] _parameters)
                => MatchParameters(_parameters, _types.Length, out IEnumerable<ParameterInfo> match) &&
                match.Zip(_types, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.IsAssignableFrom(_a)).All(_t => _t);


            bool DoesOverloadMatchExactly(ParameterInfo[] _parameters)
                => MatchParameters(_parameters, _types.Length, out IEnumerable<ParameterInfo> match) &&
                match.Zip(_types, (_p, _a) => (_a == null && _p.ParameterType.IsClass) || _p.ParameterType.Equals(_a)).All(_x => _x);

            MethodInfo[] matches = _methods.Where(_o => !HasOutParameters(_o.GetParameters()) && DoesOverloadMatch(_o.GetParameters())).ToArray();
            if (matches.Length == 1)
            {
                return matches[0];
            }
            matches = _methods.Where(_o => DoesOverloadMatchExactly(_o.GetParameters())).ToArray();
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

        private static Eval TypedInvoke(this MethodInfo _method, object _instance, object[] _arguments)
            => new Eval(_method.Invoke(_instance, _arguments), _method.ReturnType);

        #endregion

        internal static Eval InvokeOperator(Name _operator, Eval _argument)
            => GetOperator(_argument.Type, _operator, new Type[] { _argument.Type }).TypedInvoke(null, new object[] { _argument.Value });

        internal static Eval InvokeOperator(Name _operator, Eval _leftArgument, Eval _rightArgument)
        {
            Type[] argumentTypes = new Type[] { _leftArgument.Type, _rightArgument.Type };
            object[] arguments = new object[] { _leftArgument.Value, _rightArgument.Value };
            try
            {
                return GetOperator(_leftArgument.Type, _operator, argumentTypes).TypedInvoke(null, arguments);
            }
            catch { }
            return GetOperator(_rightArgument.Type, _operator, argumentTypes).TypedInvoke(null, arguments);
        }

        internal static Eval EvaluateMember(Env _env, Eval _source, Name _name)
        {
            if (_source.Value is IMemberProvider provider)
            {
                return provider.EvaluateMember(_env, _name);
            }
            bool includeInstance = _source.Value != null || c_accessInstanceMembersByType;
            bool includeStatic = _source.Value == null || c_accessStaticMembersByInstance;
            if (GetEvent(_source.Type, _name, includeStatic, includeInstance) is EventInfo eventInfo)
            {
                return new Eval(eventInfo.Equals(_env.Script.HookManager.CurrentEvent));
            }
            else if (GetProperty(_source.Type, _name, includeStatic, includeInstance) is PropertyInfo propertyInfo)
            {
                return new Eval(propertyInfo.GetValue(_source.Value), propertyInfo.PropertyType);
            }
            else if (GetField(_source.Type, _name, includeStatic, includeInstance) is FieldInfo fieldInfo)
            {
                return new Eval(fieldInfo.GetValue(_source.Value), fieldInfo.FieldType);
            }
            else if (GetNestedType(_source.Type, _name) is Type typeInfo)
            {
                return new Eval(new TypeWrapper(typeInfo));
            }
            else if (GetMethods(_source.Type, _name, includeStatic, includeInstance) is MethodInfo[] methodInfo)
            {
                return new Eval(new MethodWrapper(methodInfo, _source.Value));
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        internal static void AssignMember(Env _env, Eval _source, Name _name, Eval _value)
        {
            if (_source.Value is IAssignableMemberProvider provider)
            {
                provider.AssignMember(_env, _name, _value);
            }
            bool includeInstance = _source.Value != null || c_accessInstanceMembersByType;
            bool includeStatic = _source.Value == null || c_accessStaticMembersByInstance;
            if (GetProperty(_source.Type, _name, includeStatic, includeInstance) is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(_source.Value, _value);
            }
            else if (GetField(_source.Type, _name, includeStatic, includeInstance) is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(_source.Value, _value);
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        internal static Eval EvaluateIndexer(Eval _source, Eval[] _arguments, out Name _indexer)
        {
            try
            {
                return GetIndexerGetter(_source.Type, _arguments.GetTypes(), out _indexer).TypedInvoke(_source.Value, _arguments.GetValues());
            }
            catch
            {
                if (_source.Type.IsSubclassOf(typeof(Array)))
                {
                    _indexer = null;
                    return new Eval(((Array) _source.Value).GetValue(_arguments.GetValues().Select(_a => Convert.ToInt64(_a)).ToArray()), _source.Type.GetElementType());
                }
                else
                {
                    throw;
                }
            }
        }

        internal static void AssignIndexer(Eval _source, Eval[] _arguments, object _value, out Name _indexer)
        {
            try
            {
                object[] arguments = _arguments.GetValues().Concat(new object[] { _value }).ToArray();
                GetIndexerSetter(_source.Type, _arguments.GetTypes(), out _indexer).TypedInvoke(_source.Value, arguments);
            }
            catch (Exception)
            {
                if (_source.Type.IsSubclassOf(typeof(Array)))
                {
                    _indexer = null;
                    ((Array) _source.Value).SetValue(_value, _arguments.GetValues().Select(_a => Convert.ToInt64(_a)).ToArray());
                }
                else
                {
                    throw;
                }
            }
        }

        internal static Eval InvokeMethod(MethodInfo[] _methodGroup, object _instance, Eval[] _arguments)
        {
            MethodInfo method = ChooseOverload(_methodGroup, _arguments.GetTypes());
            return method.TypedInvoke(_instance, FillOptionalArguments(method.GetParameters(), _arguments.GetValues()).ToArray());
        }

        private static object ConvertType(Eval _source, Type _target)
        {
            Ensure.NonNull(_target, nameof(_target));
            Type[] argumentTypes = new Type[] { _source.Type };
            object[] arguments = new object[] { _source.Value };
            try
            {
                return GetOperator(_source.Type, "op_Explicit", argumentTypes).Invoke(null, arguments);
            }
            catch
            { }
            try
            {
                return GetOperator(_source.Type, "op_Implicit", argumentTypes).Invoke(null, arguments);
            }
            catch
            { }
            try
            {
                return Convert.ChangeType(_source.Value, _target);
            }
            catch
            { }
            throw new InvalidCastException();
        }

        internal static Eval ChangeType(Eval _source, Type _target)
        {
            if (_target.IsInstanceOfType(_source.Value))
            {
                return new Eval(_source.Value, _target);
            }
            else
            {
                return new Eval(ConvertType(_source, _target), _target);
            }
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
