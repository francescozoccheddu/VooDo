
using Microsoft.UI.Xaml.Media;

using System.Collections.Generic;
using System.Diagnostics;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class Animator<TValue> : Controller<TValue>
    {

        protected Animator(Variable<TValue> _variable) : this(_variable, default!) { }

        protected Animator(Variable<TValue> _variable, TValue _value) : base(_variable, _value)
        {
            SetRunning(true);
            m_Target = _value;
        }

        private bool m_running;
        private readonly Stopwatch m_stopwatch = new Stopwatch();

        private void SetRunning(bool _running)
        {
            if (m_running != _running)
            {
                m_running = _running;
                if (m_running)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    m_stopwatch.Restart();
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                    m_stopwatch.Stop();
                }
            }
        }

        private void CompositionTarget_Rendering(object? _sender, object _e) => Update();

        private void Update()
        {
            TValue value = m_Value;
            bool updated = Update(ref value, m_stopwatch.Elapsed.TotalSeconds);
            m_Value = value;
            if (updated)
            {
                m_stopwatch.Restart();
            }
            SetRunning(updated);
        }

        protected abstract bool Update(ref TValue _value, double _deltaTime);

        protected TValue m_Target { get; private set; }

        protected override void SetValue(TValue _value)
        {
            if (!EqualityComparer<TValue>.Default.Equals(_value, m_Target))
            {
                m_Target = _value;
                SetRunning(true);
            }
        }

    }

}
