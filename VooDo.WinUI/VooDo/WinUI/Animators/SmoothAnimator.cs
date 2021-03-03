using System;

namespace VooDo.WinUI.Animators
{

    public abstract class SmoothAnimator<TValue> : TargetedAnimator<TValue> where TValue : notnull
    {

        public abstract record SmoothFactory(TValue Target, double SpeedFactor, double MinDifference) : TargetedFactory(Target);

        protected SmoothAnimator(TValue _value, TValue _target) : base(_value, _target)
        { }

        public const double defaultSpeedFactor = 100;
        public const double defaultMinDifference = double.Epsilon * 100;

        public double SpeedFactor { get; protected set; } = 100;
        public double MinDifference { get; protected set; } = double.Epsilon * 100;

        protected sealed override TValue Update(double _deltaTime, TValue _current, TValue _target)
            => Smooth(_current, _target, Math.Min(_deltaTime * SpeedFactor, 1));

        protected double SmoothScalar(double _current, double _target, double _alpha)
        {
            _current = (_current * (1 - _alpha)) + (_target * _alpha);
            if (Math.Abs(_current - _target) < MinDifference)
            {
                _current = _target;
            }
            return _current;
        }

        protected abstract TValue Smooth(TValue _current, TValue _target, double _alpha);

    }

    public sealed class DoubleSmoothAnimator : SmoothAnimator<double>
    {

        public sealed record DoubleSmoothFactory(double Target, double SpeedFactor, double MinDifference) : SmoothFactory(Target, SpeedFactor, MinDifference)
        {
            protected override Animator<double> Create(double _value)
                => new DoubleSmoothAnimator(_value, Target)
                {
                    SpeedFactor = SpeedFactor,
                    MinDifference = MinDifference
                };
        }

        public DoubleSmoothAnimator(double _value, double _target) : base(_value, _target)
        { }

        protected override double Smooth(double _current, double _target, double _alpha)
            => SmoothScalar(_current, _target, _alpha);

    }

}
