using System;
using System.Collections.Generic;

namespace VooDo.Transformation
{

    public abstract class Controller<TValue>
    {

        private TValue m_value;

        internal Variable<TValue> Variable { get; private set; }

        protected Controller(Variable<TValue> _variable)
        {
            if (Variable == null)
            {
                throw new ArgumentNullException(nameof(_variable));
            }
            Variable = _variable;
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

    }

}
