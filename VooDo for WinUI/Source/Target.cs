
using System;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Runtime;

namespace VooDo.WinUI
{

    public delegate void TargetDiscontinuedEventHandler(Target _target);

    public abstract class Target
    {

        internal Target() { }

        protected internal virtual void AttachProgram(Program _program) { }
        protected internal virtual void DetachProgram() { }
        protected internal virtual Script ProcessScript(Script _script) => _script;
        public virtual Type ReturnType => typeof(void);

        public event TargetDiscontinuedEventHandler? OnTargetDiscontinued;

        protected void NotifyDiscontinued()
            => OnTargetDiscontinued?.Invoke(this);

    }

    public abstract class SimpleTarget : Target
    {

        public SimpleTarget(IReturnTarget? _returnTarget, ImmutableArray<IConstantValue> _constants)
        {
            m_returnTarget = _returnTarget;
            m_constants = _constants;
        }

        private readonly IReturnTarget? m_returnTarget;
        private readonly ImmutableArray<IConstantValue> m_constants;

        private Program? m_program;

        protected internal sealed override void AttachProgram(Program _program)
        {
            m_program = _program;
            if (m_returnTarget is not null)
            {
                ((TypedProgram) m_program).OnReturn += m_returnTarget.SetReturnValue;
            }
        }

        protected internal sealed override void DetachProgram()
        {
            m_program = null;
            if (m_returnTarget is not null)
            {
                ((TypedProgram) m_program!).OnReturn -= m_returnTarget.SetReturnValue;
            }
        }

        public sealed override Type? ReturnType => m_returnTarget?.ReturnType;
        protected internal sealed override Script ProcessScript(Script _script)
        {
            return _script; // TODO Add constants
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
