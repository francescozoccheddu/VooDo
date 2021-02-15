using System;
using System.Linq;
using System.Reflection;

using VooDo.AST.Names;

namespace VooDo.Runtime
{

    public sealed class Loader
    {

        public static Loader FromType(Type _type)
            => new Loader(_type);

        public static Loader FromAssembly(Assembly _assembly)
            => new Loader(_assembly.GetTypes().Single(_t => _t.IsSubclassOf(typeof(Program))));

        public static Loader FromAssembly(Assembly _assembly, Namespace _namespace, Identifier _className)
            => new Loader(_assembly.GetType($"{_namespace}.{_className}", true)!);

        private readonly Type m_type;

        private Loader(Type _type)
        {
            if (!_type.IsSubclassOf(typeof(Program)))
            {
                throw new ArgumentException("Not a Program", nameof(_type));
            }
            if (_type.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new ArgumentException("Type does not have a parameterless constructor", nameof(_type));
            }
            m_type = _type;
        }

        public Program Create()
            => (Program) Activator.CreateInstance(m_type)!;

        public Program<TReturn> Create<TReturn>()
            => (Program<TReturn>) Create();

    }

}
