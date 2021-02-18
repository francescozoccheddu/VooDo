using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace VooDo.WinUI.Attributes
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public abstract class TypeTagAttribute : Attribute
    {

        internal readonly struct Application
        {

            public Application(TypeTagAttribute _attribute, Type _target)
            {
                Attribute = _attribute;
                Target = _target;
            }

            public TypeTagAttribute Attribute { get; }
            public Type Target { get; }

        }

        internal TypeTagAttribute() { }

        internal static ImmutableDictionary<Type, ImmutableArray<Application>> RetrieveAttributes(ImmutableArray<Assembly> _assemblies)
            => _assemblies
                .SelectMany(_a => _a.GetTypes())
                .SelectMany(_t => _t.GetCustomAttributes<TypeTagAttribute>().Select(_ta => new Application(_ta, _t)))
                .GroupBy(_a => _a.Attribute.GetType())
                .ToImmutableDictionary(_g => _g.Key, _g => _g.ToImmutableArray());

        internal static TType Instantiate<TType>(Type _type, params object[] _arguments)
        {
            try
            {
                return (TType) Activator.CreateInstance(_type, _arguments)!;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to instantiate {typeof(TType).Name} by calling constructor " +
                    $"{_type.Name}({string.Join(", ", _arguments.Select(_a => _a?.GetType().Name ?? "null"))})", exception);
            }
        }

    }

    public abstract class SingletonTypeTagAttribute : TypeTagAttribute
    {

        public int Priority { get; set; } = 0;

        internal SingletonTypeTagAttribute() { }

        internal static bool Resolve(IEnumerable<Application> _applications, out Application _application)
        {
            int maxPriority = int.MinValue;
            bool found = false;
            _application = default;
            foreach (Application application in _applications)
            {
                int priority = ((SingletonTypeTagAttribute) application.Attribute).Priority;
                if (priority >= maxPriority)
                {
                    found = true;
                    maxPriority = priority;
                    _application = application;
                }
            }
            return found;
        }

    }

    public abstract class CombinableTypeTagAttribute : TypeTagAttribute
    {

        public enum EKind
        {
            Combinable, Standalone
        }

        public int Priority { get; set; } = 0;
        public EKind Kind { get; set; } = EKind.Combinable;

        internal CombinableTypeTagAttribute() { }

        internal static bool Resolve(IEnumerable<Application> _applications, out ImmutableArray<Application> _resolvedApplications, out EKind _kind)
        {
            _kind = EKind.Combinable;
            Application standalone = default;
            List<Application> applications = new();
            int maxPriority = int.MinValue;
            foreach (Application application in _applications)
            {
                CombinableTypeTagAttribute attribute = (CombinableTypeTagAttribute) application.Attribute;
                if (attribute.Kind == EKind.Combinable)
                {
                    if (_kind == EKind.Combinable)
                    {
                        applications.Add(application);
                    }
                }
                else
                {
                    _kind = EKind.Standalone;
                    if (attribute.Priority >= maxPriority)
                    {
                        maxPriority = attribute.Priority;
                        standalone = application;
                    }
                }
            }
            if (_kind == EKind.Combinable)
            {
                _resolvedApplications = applications
                    .OrderByDescending(_a => ((CombinableTypeTagAttribute) _a.Attribute).Priority)
                    .ToImmutableArray();
            }
            else
            {
                _resolvedApplications = ImmutableArray.Create(standalone);
            }
            return !_resolvedApplications.IsEmpty;
        }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public abstract class AssemblyTagAttribute : Attribute
    {

        internal AssemblyTagAttribute() { }

        internal static ImmutableDictionary<Type, ImmutableArray<AssemblyTagAttribute>> RetrieveAttributes(ImmutableArray<Assembly> _assemblies)
            => _assemblies
                .SelectMany(_a => _a.GetCustomAttributes<AssemblyTagAttribute>())
                .GroupBy(_aa => _aa.GetType())
                .ToImmutableDictionary(_g => _g.Key, _g => _g.ToImmutableArray());

    }

}
