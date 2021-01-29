using System;

namespace VooDo.Transformation
{

    public delegate void VariableChangedEventHandler(Variable _variable, object _oldValue);

    public delegate void VariableChangedEventHandler<TValue>(Variable<TValue> _variable, TValue _oldValue);

    public abstract class Variable
    {

        internal Variable(string _name, Type _type)
        {
            if (_name == null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            Name = _name;
            Type = _type;
        }

        public string Name { get; }
        public Type Type { get; }
        public object Value { get => m_DynamicValue; set => m_DynamicValue = value; }
        public abstract bool HasController { get; }

        public Variable<TValue> OfType<TValue>() => (Variable<TValue>) this;

        public event VariableChangedEventHandler OnChange;

        internal abstract void NotifyChanged();

        protected abstract object m_DynamicValue { get; set; }

        protected void NotifyChanged(object _oldValue) => OnChange?.Invoke(this, _oldValue);

    }

    public sealed class Variable<TValue> : Variable
    {

        private sealed class NoController : Controller<TValue>
        {

            internal NoController(Variable<TValue> _variable) : base(_variable)
            {
            }

            public override IControllerFactory<TValue> Factory => throw new NotImplementedException();

            protected override void SetValue(TValue _value) => m_Value = _value;

        }

        private TValue m_oldValue;

        private Controller<TValue> m_controller;

        internal Variable(string _name) : base(_name, typeof(TValue)) => SetController(null);

        public new TValue Value { get => m_controller.Value; set => m_controller.Value = value; }

        public Controller<TValue> Controller => m_controller is NoController ? null : m_controller;

        public override bool HasController => Controller != null;

        public void SetController(IControllerFactory<TValue> _factory)
        {
            Controller<TValue> controller = _factory?.Create(this) ?? new NoController(this);
            if (controller.Variable != this)
            {
                throw new Exception("Controller is not bound to this variable");
            }
            m_controller?.Destroy();
            m_controller = controller;
        }

        public new event VariableChangedEventHandler<TValue> OnChange;

        internal override void NotifyChanged()
        {
            TValue oldValue = m_oldValue;
            NotifyChanged(m_oldValue);
            OnChange?.Invoke(this, m_oldValue);
            m_oldValue = Value;
        }

        protected override object m_DynamicValue { get => Value; set => Value = (TValue) value; }

    }

}
