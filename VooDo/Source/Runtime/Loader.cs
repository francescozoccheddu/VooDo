using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VooDo.AST.Names;

namespace VooDo.Runtime
{

    public sealed class Loader : IEquatable<Loader?>
    {

        public static Loader FromType(Type _type)
            => new Loader(_type);

        public static Loader FromAssembly(Assembly _assembly)
            => new Loader(_assembly.GetTypes().Single(_t => _t.IsSubclassOf(typeof(Program))));

        public static Loader FromAssembly(Assembly _assembly, Namespace _namespace, Identifier _className)
            => new Loader(_assembly.GetType($"{_namespace}.{_className}", true)!);

        private readonly Type m_type;

        public Type ReturnType { get; }
        public bool IsTyped => ReturnType != typeof(void);

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
            ReturnType = m_type
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(_m => _m.IsVirtual
                    && _m.Name is nameof(Program.Run) or nameof(TypedProgram<object>.TypedRun)
                    && _m.GetBaseDefinition().DeclaringType!.IsSubclassOf(typeof(Program)))
                .ReturnType;
        }

        public Program Create()
            => (Program) Activator.CreateInstance(m_type)!;

        public TypedProgram<TReturn> Create<TReturn>()
            => (TypedProgram<TReturn>) Create();

        public override bool Equals(object? _obj) => Equals(_obj as Loader);
        public bool Equals(Loader? _other) => _other is not null && m_type == _other.m_type;
        public override int GetHashCode() => HashCode.Combine(m_type);

        public static bool operator ==(Loader? _left, Loader? _right) => EqualityComparer<Loader>.Default.Equals(_left, _right);
        public static bool operator !=(Loader? _left, Loader? _right) => !(_left == _right);

    }

}
