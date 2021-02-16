using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace VooDo.WinUI
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public abstract class ServiceProviderAttribute : Attribute
    {

        public int Priority { get; }

        internal ServiceProviderAttribute() { }

        internal static (TService provider, int priority)? GetProvider<TService, TAttribute>()
            where TService : notnull
            where TAttribute : ServiceProviderAttribute
        {
            (Type type, int priority)[] candidates = GetProviderTypes<TService, TAttribute>()
                .Take(2)
                .Select(_e => (_e.type, _e.attribute.Priority))
                .ToArray();
            Type? candidate = null;
            if (candidates.Length == 1)
            {
                candidate = candidates[0].type;
            }
            else if (candidates.Length > 2)
            {
                if (candidates[0].priority == candidates[1].priority)
                {
                    throw new InvalidOperationException($"Cannot choose {typeof(TService).Name} between {candidates[0].type.Name} and {candidates[1].type.Name} because they have the same priority");
                }
                candidate = candidates[0].type;
            }
            if (candidate is not null)
            {
                return (Instantiate<TService>(candidate), candidates[0].priority);
            }
            return null;
        }

        private static TService Instantiate<TService>(Type _type)
        {
            try
            {
                return (TService) Activator.CreateInstance(_type)!;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to instantiate {typeof(TService).Name} of type {_type.Name} via parameterless constructor", exception);
            }
        }

        internal static ImmutableArray<(TService provider, int priority)> GetProviders<TService, TAttribute>()
            where TService : notnull
            where TAttribute : ServiceProviderAttribute
            => GetProviderTypes<TService, TAttribute>()
                .Select(_e => (Instantiate<TService>(_e.type), _e.attribute.Priority))
                .ToImmutableArray();

        private protected static ParallelQuery<(Type type, TAttribute attribute)> GetProviderTypes<TService, TAttribute>()
            where TService : notnull
            where TAttribute : ServiceProviderAttribute
        {
            Assembly attributeAssembly = typeof(TService).Assembly;
            AssemblyName attributeAssemblyName = attributeAssembly.GetName();
            return
                from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                where assembly == attributeAssembly || assembly.GetReferencedAssemblies().Contains(attributeAssemblyName)
                from type in assembly.GetTypes()
                where IsDefined(type, typeof(TService)) && type.IsAssignableTo(typeof(TService))
                let attribute = (TAttribute) type.GetCustomAttributes(typeof(TAttribute), false).Single()
                let priority = ((TAttribute) type.GetCustomAttributes(typeof(TAttribute), false).Single()).Priority
                orderby priority descending
                select (type, attribute);
        }

    }

    public abstract class CombinableServiceProviderAttribute : ServiceProviderAttribute
    {

        public enum EKind
        {
            Combinable, Standalone
        }

        internal CombinableServiceProviderAttribute() { }

        public EKind Kind { get; set; } = EKind.Combinable;

        internal static (TService provider, int priority)? GetProvider<TService, TAttribute>()
            where TService : notnull
            where TAttribute : ServiceProviderAttribute
        {

        }

    }
