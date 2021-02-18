using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;
using VooDo.Utils;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Components
{

    public class SimpleTarget : ITarget
    {

        public SimpleTarget(IReturnTarget? _returnTarget = null, ImmutableArray<IConstantValue> _constants = default)
        {
            m_returnTarget = _returnTarget;
            m_constants = _constants.EmptyIfDefault();
        }

        event TargetDiscontinuedEventHandler? ITarget.OnTargetDiscontinued
        {
            add => OnTargetDiscontinued += value;
            remove => OnTargetDiscontinued -= value;
        }

        internal event TargetDiscontinuedEventHandler? OnTargetDiscontinued;

        private readonly IReturnTarget? m_returnTarget;
        private readonly ImmutableArray<IConstantValue> m_constants;

        private Program? m_program;

        internal void AttachProgram(Program _program)
        {
            m_program = _program;
            if (m_returnTarget is not null)
            {
                ((TypedProgram) m_program).OnReturn += m_returnTarget.SetReturnValue;
            }
        }

        internal void DetachProgram()
        {
            m_program = null;
            if (m_returnTarget is not null)
            {
                ((TypedProgram) m_program!).OnReturn -= m_returnTarget.SetReturnValue;
            }
        }

        public virtual Type ReturnType => m_returnTarget?.ReturnType ?? typeof(void);

        internal virtual Script ProcessScript(Script _script)
        {
            return _script; // TODO Add constants
        }

        protected void NotifyTargetDiscontinued()
        {
            OnTargetDiscontinued?.Invoke(this);
        }

    }

    public interface IConstantValue
    {

        Type Type { get; }
        Identifier Name { get; }
        object? GetValue();

    }

    public sealed class ConstantValue<TValue> : IConstantValue
    {

        public ConstantValue(Identifier _name, TValue _value)
            : this(_name, _value, typeof(TValue))
        {

        }

        public ConstantValue(Identifier _name, TValue _value, Type _type)
        {
            Name = _name;
            Value = _value;
            Type = _type;
        }

        public Type Type { get; }
        public Identifier Name { get; }
        public TValue Value { get; }

        Type IConstantValue.Type => typeof(TValue);
        Identifier IConstantValue.Name => Name;
        object? IConstantValue.GetValue() => Value;

    }

    public interface IReturnTarget
    {

        Type ReturnType { get; }
        void SetReturnValue(object? _value);

    }

    public abstract class ReturnTarget<TValue> : IReturnTarget
    {

        protected abstract TValue m_Value { set; }

        protected virtual Type m_ReturnType => typeof(TValue);

        Type IReturnTarget.ReturnType => m_ReturnType;
        void IReturnTarget.SetReturnValue(object? _value) => m_Value = (TValue) _value!;

    }

}
