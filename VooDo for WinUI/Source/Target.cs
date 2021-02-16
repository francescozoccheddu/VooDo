
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

        protected virtual void AttachVariable(Variable _variable) { }
        protected virtual void DetachVariable() { }
        protected virtual Script ProcessScript(Script _script) => _script;
        protected virtual Type? m_ReturnType => null;

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

        private Variable? m_variable;


        protected sealed override void AttachVariable(Variable _variable)
        {
            m_variable = _variable;
        }

        protected sealed override void DetachVariable()
        {
            m_variable = null;
        }

        protected sealed override Type? m_ReturnType => m_returnTarget?.ReturnType;
        protected sealed override Script ProcessScript(Script _script)
        {
            throw new NotImplementedException();
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
