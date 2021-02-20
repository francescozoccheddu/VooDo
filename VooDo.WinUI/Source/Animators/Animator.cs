
using System.Collections.Generic;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class Animator<TValue> : Controller<TValue>, IAnimator
    {

        protected Animator(Variable<TValue> _variable) : this(_variable, default!) { }

        protected Animator(Variable<TValue> _variable, TValue _value) : base(_variable, _value)
        {
            SetRunning(true);
            m_Target = _value;
        }

        private bool m_running;

        private void SetRunning(bool _running)
        {
            if (m_running != _running)
            {
                m_running = _running;
                if (m_running)
                {
                    AnimatorManager.RegisterAnimator(this);
                }
                else
                {
                    AnimatorManager.UnregisterAnimator(this);
                }
            }
        }

        protected abstract bool Update(ref TValue _value, double _deltaTime);

        protected TValue m_Target { get; private set; }

        protected sealed override void SetValue(TValue _value)
        {
            if (!EqualityComparer<TValue>.Default.Equals(_value, m_Target))
            {
                m_Target = _value;
                SetRunning(true);
            }
        }

        protected sealed override void Destroying() => SetRunning(false);

        Program IAnimator.Program => Variable.Program;

        void IAnimator.Update(double _deltaTime)
        {
            TValue value = m_Value;
            bool updated = Update(ref value, _deltaTime);
            m_Value = value;
            SetRunning(updated);
        }

    }

}
