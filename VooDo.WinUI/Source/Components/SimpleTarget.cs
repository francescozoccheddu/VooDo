using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Components
{

    public abstract class SimpleTarget : ITarget
    {

        public SimpleTarget(IReturnTarget? _returnTarget, ImmutableArray<IConstantValue> _constants)
        {
            m_returnTarget = _returnTarget;
            m_constants = _constants;
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
        {
            Name = _name;
            Value = _value;
        }

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

        Type IReturnTarget.ReturnType => typeof(TValue);
        void IReturnTarget.SetReturnValue(object? _value) => m_Value = (TValue) _value!;

    }

}
