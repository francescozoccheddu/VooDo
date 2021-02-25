using System;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public sealed class SmoothAnimator : Animator<double>
    {

        private static SmoothAnimator GetOrCreate(Variable<double> _variable)
        {
            if (_variable.Controller is SmoothAnimator animator)
            {
                return animator;
            }
            else
            {
                return new SmoothAnimator(_variable, _variable.Value);
            }
        }

        public sealed class Factory : IControllerFactory<double>
        {

            public double Target { get; }

            public Factory(double _target)
            {
                Target = _target;
            }

            public Controller<double> Create(Variable<double> _variable)
            {
                SmoothAnimator controller = GetOrCreate(_variable);
                controller.SetValue(Target);
                return controller;
            }

        }

        public SmoothAnimator(Variable<double> _variable) : base(_variable)
        {
        }

        public SmoothAnimator(Variable<double> _variable, double _value) : base(_variable, _value)
        {
        }

        public override Controller<double> Create(Variable<double> _variable)
            => GetOrCreate(_variable);

        protected override bool Update(ref double _value, double _deltaTime)
        {
            double alpha = Math.Min(_deltaTime * 10, 1);
            _value = (_value * (1 - alpha)) + (m_Target * alpha);
            if (Math.Abs(_value - m_Target) < 0.0001)
            {
                _value = m_Target;
                return false;
            }
            return true;
        }

    }

}
