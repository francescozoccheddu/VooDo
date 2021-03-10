using System;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class SmoothAnimator<TValue> : NoOvershootTargetedAnimator<TValue> where TValue : notnull
    {

        public abstract record SmoothFactory<TAnimator>(TValue Target, double SpeedFactor, double MinDifference) : TargetedFactory<TAnimator>(Target) where TAnimator : SmoothAnimator<TValue>
        {

            protected override void Set(TAnimator _animator, IController<TValue> _oldController)
            {
                base.Set(_animator, _oldController);
                _animator.SpeedFactor = SpeedFactor;
                _animator.MinDifference = MinDifference;
            }

        }

        protected SmoothAnimator(TValue _value, TValue _target) : base(_value, _target)
        { }

        public const double defaultSpeedFactor = 10;
        public const double defaultMinDifference = double.Epsilon * 100;

        public double SpeedFactor { get; protected set; } = defaultSpeedFactor;
        public double MinDifference { get; protected set; } = defaultMinDifference;

        protected sealed override TValue Update(double _deltaTime, TValue _current, TValue _target)
            => Smooth(_current, _target, Math.Min(_deltaTime * SpeedFactor, 1), _deltaTime);

        protected double SmoothScalar(double _current, double _target, double _alpha)
        {
            _current = (_current * (1 - _alpha)) + (_target * _alpha);
            if (Math.Abs(_current - _target) < MinDifference)
            {
                _current = _target;
            }
            return _current;
        }

        protected abstract TValue Smooth(TValue _current, TValue _target, double _alpha, double _deltaTime);

    }

    public sealed class DoubleSmoothAnimator : SmoothAnimator<double>, IAnimatorWithVelocity<double>
    {

        public sealed record DoubleSmoothFactory(double Target, double SpeedFactor, double MinDifference) : SmoothFactory<DoubleSmoothAnimator>(Target, SpeedFactor, MinDifference)
        {
            protected override DoubleSmoothAnimator Create(double _value) => new(_value, Target);
        }

        public DoubleSmoothAnimator(double _value, double _target) : base(_value, _target)
        { }

        private double m_speed;

        double IAnimatorWithVelocity<double>.Velocity => m_speed;

        protected override double Smooth(double _current, double _target, double _alpha, double _deltaTime)
        {
            double oldValue = _current;
            double newValue = SmoothScalar(_current, _target, _alpha);
            if (_deltaTime != 0)
            {
                m_speed = (newValue - oldValue) / _deltaTime;
            }
            return newValue;
        }
    }

}
