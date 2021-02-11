using System.Collections.Generic;

namespace VooDo.Runtime
{

    public abstract class Controller<TValue> : IControllerFactory<TValue>
    {

        private TValue m_value;

        internal Variable<TValue>? Variable { get; private set; }

        protected Controller(Variable<TValue> _variable) : this(_variable, _variable.Value)
        { }

        protected Controller(Variable<TValue> _variable, TValue _value)
        {
            Variable = _variable;
            m_value = _value;
        }

        public TValue Value { get => m_Value; set => SetValue(value); }

        public abstract IControllerFactory<TValue> Factory { get; }

        protected TValue m_Value
        {
            get => m_value;
            set
            {
                if (!EqualityComparer<TValue>.Default.Equals(m_value, value))
                {
                    m_value = value;
                    Variable?.NotifyChanged();
                }
            }
        }

        protected abstract void SetValue(TValue _value);

        internal void Destroy() => Variable = null;

        public abstract Controller<TValue> Create(Variable<TValue> _variable);

    }

}
