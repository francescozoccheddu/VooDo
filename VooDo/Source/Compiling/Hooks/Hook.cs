using System;
using System.Collections.Generic;

namespace VooDo.Hooks
{

    public abstract class Hook<TObject, TToken> : IHook where TObject : class
    {

        private IHookListener? m_listener;
        private TObject? m_object;
        private TToken m_token = default!;
        IHookListener? IHook.Listener { set => m_listener = value; }

        protected abstract TToken Subscribe(TObject _object);
        protected abstract void Unsubscribe(TToken _token);
        protected virtual void NotifyChange() => m_listener?.NotifyChange();
        public abstract IHook Clone();

        void IHook.Subscribe(object _object)
        {
            TObject newObject = (TObject) _object;
            if (!ReferenceEquals(newObject, m_object))
            {
                if (m_object is not null)
                {
                    Unsubscribe(m_token);
                }
                m_object = newObject;
                m_token = Subscribe(newObject);
            }
        }

        void IHook.Unsubscribe()
        {
            if (m_object is not null)
            {
                Unsubscribe(m_token);
                m_object = null;
            }
        }

    }

    public abstract class Hook<TObject, TToken, TValue> : Hook<TObject, TToken> where TObject : class
    {

        private bool m_initialized;
        private TValue m_value;
        private readonly IEqualityComparer<TValue> m_comparer;

        protected Hook() : this(default(TValue)!)
        {
            m_initialized = false;
        }

        protected Hook(TValue _value) : this(_value, EqualityComparer<TValue>.Default)
        {
        }

        protected Hook(IEqualityComparer<TValue> _comparer) : this(default!, _comparer)
        {
            m_initialized = false;
        }

        protected Hook(TValue _initialValue, IEqualityComparer<TValue> _comparer)
        {
            m_initialized = true;
            m_value = _initialValue;
            m_comparer = _comparer;
        }

        protected override void NotifyChange() => throw new NotSupportedException();

        protected void NotifyChange(TValue _value)
        {
            if (!m_initialized || !m_comparer.Equals(_value, m_value))
            {
                m_initialized = true;
                m_value = _value;
                base.NotifyChange();
            }
        }

    }

}
