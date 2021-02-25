using System;
using System.Collections.Generic;

namespace VooDo.Runtime
{

    public abstract class Controller<TValue> : IControllerFactory<TValue>
    {

        private TValue m_value;

        public Variable<TValue> Variable { get; }
        public bool Destroyed { get; private set; }

        protected Controller(Variable<TValue> _variable) : this(_variable, _variable.Value)
        { }

        protected Controller(Variable<TValue> _variable, TValue _value)
        {
            m_value = _value;
            Variable = _variable;
        }

        public TValue Value { get => m_Value; set => SetValue(value); }

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

        protected virtual void Destroying() { }

        internal void Destroy()
        {
            if (!Destroyed)
            {
                Destroyed = true;
                Destroying();
            }
            else
            {
                throw new InvalidOperationException("Already destroyed");
            }
        }

        public abstract Controller<TValue> Create(Variable<TValue> _variable);

    }

}
