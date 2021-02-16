using System;
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
            Assembly attributeAssembly = typeof(TService).Assembly;
            AssemblyName attributeAssemblyName = attributeAssembly.GetName();
            (Type type, int priority)[] candidates = (
                from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                where assembly == attributeAssembly || assembly.GetReferencedAssemblies().Contains(attributeAssemblyName)
                from type in assembly.GetTypes()
                where IsDefined(type, typeof(TService)) && type.IsAssignableTo(typeof(TService))
                let priority = ((TAttribute) type.GetCustomAttributes(typeof(TAttribute), false).Single()).Priority
                orderby priority descending
                select (type, priority))
                .Take(2).ToArray();
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
                try
                {
                    return ((TService) Activator.CreateInstance(candidate)!, candidates[0].priority);
                }
                catch
                {
                    throw new InvalidOperationException($"Failed to instantiate {typeof(TService).Name} of type {candidate.Name} via parameterless constructor");
                }
            }
            return null;
        }

    }

}
