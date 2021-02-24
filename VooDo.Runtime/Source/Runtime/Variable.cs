using System;

namespace VooDo.Runtime
{

    public delegate void VariableChangedEventHandler(Variable _variable, object? _oldValue);

    public delegate void VariableChangedEventHandler<TValue>(Variable<TValue> _variable, TValue _oldValue);

    public abstract class Variable
    {

        internal Variable(bool _isConstant, string _name, Type _type)
        {
            IsConstant = _isConstant;
            Name = _name;
            Type = _type;
        }

        public bool IsConstant { get; }
        public string Name { get; }
        public Type Type { get; }
        public object? Value { get => m_DynamicValue; set => m_DynamicValue = value; }
        public object? ControllerFactory { get => m_DynamicControllerFactory; set => m_DynamicControllerFactory = value; }
        public abstract bool HasController { get; }
        public Program Program { get; internal set; } = null!;

        public Variable<TValue> OfType<TValue>() => (Variable<TValue>) this;

        public event VariableChangedEventHandler? OnChange;

        internal abstract void NotifyChanged();

        protected abstract object? m_DynamicValue { get; set; }
        protected abstract object? m_DynamicControllerFactory { get; set; }

        protected void NotifyChanged(object? _oldValue)
        {
            Program.RequestRun();
            OnChange?.Invoke(this, _oldValue);
        }

    }

    public sealed class Variable<TValue> : Variable
    {

        private sealed class NoController : Controller<TValue>
        {

            internal NoController(Variable<TValue> _variable) : base(_variable) { }
            internal NoController(Variable<TValue> _variable, TValue _value) : base(_variable, _value) { }

            protected override void SetValue(TValue _value) => m_Value = _value;

            public override Controller<TValue> Create(Variable<TValue> _variable) => throw new NotSupportedException();

        }

        private TValue m_oldValue;

        private Controller<TValue> m_controller;

        internal Variable(bool _isConstant, string _name, TValue _value = default) : base(_isConstant, _name, typeof(TValue))
        {
            m_oldValue = _value!;
            m_controller = new NoController(this, _value!);
        }


        public new TValue Value { get => m_controller.Value; set => m_controller.Value = value; }

        public Controller<TValue>? Controller => m_controller is NoController ? null : m_controller;
        public new IControllerFactory<TValue>? ControllerFactory
        {
            get => Controller;
            set
            {
                Controller<TValue> controller = value?.Create(this) ?? new NoController(this);
                if (controller.Variable != this)
                {
                    throw new Exception("Controller is not bound to this variable");
                }
                if (!ReferenceEquals(m_controller, controller))
                {
                    m_controller?.Destroy();
                }
                m_controller = controller;
            }
        }

        public override bool HasController => ControllerFactory is not null;

        public new event VariableChangedEventHandler<TValue>? OnChange;

        internal override void NotifyChanged()
        {
            TValue oldValue = m_oldValue;
            m_oldValue = Value;
            NotifyChanged(oldValue);
            OnChange?.Invoke(this, oldValue);
        }

        protected override object? m_DynamicValue { get => Value; set => Value = (TValue) value!; }
        protected override object? m_DynamicControllerFactory { get => ControllerFactory; set => ControllerFactory = (IControllerFactory<TValue>?) value; }

    }

}
