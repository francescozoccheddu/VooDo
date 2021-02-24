﻿using System;
using System.Linq;
using System.Reflection;

namespace VooDo.Runtime
{

    public sealed class Loader : IEquatable<Loader?>
    {

        public static Loader FromType(Type _type)
            => new Loader(_type);

        public static Loader FromAssembly(Assembly _assembly)
            => new Loader(_assembly.GetTypes().Single(_t => _t.IsSubclassOf(typeof(Program))));

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
                .Single(_m => _m.IsVirtual && !_m.IsFinal
                    && _m.Name is nameof(Program.Run) or nameof(TypedProgram<object>.TypedRun)
                    && _m.GetBaseDefinition().DeclaringType is Type declaring
                    && (declaring == typeof(Program) || declaring.IsSubclassOf(typeof(Program))))
                .ReturnType;
        }

        public Program Create()
            => (Program) Activator.CreateInstance(m_type)!;

        public TypedProgram<TReturn> Create<TReturn>()
            => (TypedProgram<TReturn>) Create();

        public override bool Equals(object? _obj) => Equals(_obj as Loader);
        public bool Equals(Loader? _other) => _other is not null && m_type == _other.m_type;
        public override int GetHashCode() => m_type.GetHashCode();

        public static bool operator ==(Loader? _left, Loader? _right) => _left is not null && _left.Equals(_right);
        public static bool operator !=(Loader? _left, Loader? _right) => !(_left == _right);

    }

}