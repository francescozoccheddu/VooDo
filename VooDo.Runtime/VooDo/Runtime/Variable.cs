using System;
using System.Collections.Generic;

namespace VooDo.Runtime
{

    public delegate void VariableChangedEventHandler(IVariable _variable, object? _oldValue);

    public delegate void VariableChangedEventHandler<TValue>(Variable<TValue> _variable, TValue? _oldValue) where TValue : notnull;

    public interface IVariable
    {

        IProgram Program { get; internal set; }
        bool IsConstant { get; }
        string Name { get; }
        Type Type { get; }
        object? Value { get; set; }
        IController Controller { get; }
        IControllerFactory ControllerFactory { get; set; }
        bool HasDefaultController { get; }
        void NotifyChanged();

        event VariableChangedEventHandler OnValueChange;

    }

    public sealed class Variable<TValue> : IVariable where TValue : notnull
    {

        private sealed class NoController : IController<TValue>
        {

            internal TValue? value;

            internal NoController(TValue? _value)
            {
                value = _value;
            }

            TValue? IController<TValue>.Value => value;
            object? IController.Value => value;

            IController<TValue> IControllerFactory<TValue>.CreateController(Variable<TValue> _variable)
            {
                _variable.m_noController.value = value;
                return _variable.m_noController;
            }

            IController IControllerFactory.CreateController(IVariable _variable)
                => ((IControllerFactory<TValue>)this).CreateController((Variable<TValue>)_variable);

            void IController<TValue>.OnAttach(Variable<TValue> _variable) { }
            void IController<TValue>.OnDetach(Variable<TValue> _variable) { }
            void IController.OnAttach(IVariable _variable) { }
            void IController.OnDetach(IVariable _variable) { }

        }

        private TValue? m_value;
        private readonly NoController m_noController;

        public string Name { get; }
        public bool IsConstant { get; }


        internal Variable(bool _isConstant, string _name, TValue? _value = default)
        {
            m_value = _value;
            Controller = m_noController = new NoController(_value);
            IsConstant = _isConstant;
            Name = _name;
        }

        public IProgram Program { get; internal set; } = null!;

        public bool HasDefaultController => Controller == m_noController;

        public IController<TValue> Controller { get; private set; }

        public IControllerFactory<TValue> ControllerFactory
        {
            get => Controller;
            set
            {
                IController<TValue> newController = value.CreateController(this);
                if (!ReferenceEquals(newController, Controller))
                {
                    Controller.OnDetach(this);
                    Controller = newController;
                    Controller.OnAttach(this);
                }
                NotifyChanged();
            }
        }
        public TValue? Value
        {
            get => m_value;
            set
            {
                m_noController.value = value;
                ControllerFactory = m_noController;
            }
        }

        public void NotifyChanged()
        {
            TValue? oldValue = m_value;
            m_value = Controller.Value;
            if (EqualityComparer<TValue?>.Default.Equals(m_value, oldValue))
            {
                OnValueChange?.Invoke(this, oldValue);
                m_OnValueChangeDynamic?.Invoke(this, oldValue);
                Program.RequestRun();
            }
        }

        public event VariableChangedEventHandler<TValue>? OnValueChange;
        private event VariableChangedEventHandler? m_OnValueChangeDynamic;

        event VariableChangedEventHandler IVariable.OnValueChange
        {
            add => m_OnValueChangeDynamic += value;
            remove => m_OnValueChangeDynamic -= value;
        }
        Type IVariable.Type => typeof(TValue);
        object? IVariable.Value { get => Value; set => Value = (TValue?)value; }
        IController IVariable.Controller => Controller;
        IControllerFactory IVariable.ControllerFactory { get => ControllerFactory; set => ControllerFactory = (IControllerFactory<TValue>)value; }

        IProgram IVariable.Program { get => Program; set => Program = value; }

    }

    public static class VariableExtensions
    {

        public static Variable<TValue> OfType<TValue>(this IVariable _variable) where TValue : notnull
            => (Variable<TValue>)_variable;

        public static void Freeze(this IVariable _variable) => _variable.Value = _variable.Value;

    }

}
