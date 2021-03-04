﻿using System;

namespace VooDo.WinUI.Animators
{

    public abstract class SmoothAnimator<TValue> : TargetedAnimator<TValue> where TValue : notnull
    {

        public abstract record SmoothFactory<TAnimator>(TValue Target, double SpeedFactor, double MinDifference) : TargetedFactory<TAnimator>(Target) where TAnimator : SmoothAnimator<TValue>
        {

            protected override void Set(TAnimator _animator)
            {
                base.Set(_animator);
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

        public sealed record DoubleSmoothFactory(double Target, double SpeedFactor, double MinDifference) : SmoothFactory<DoubleSmoothAnimator>(Target, SpeedFactor, MinDifference)
        {
            protected override DoubleSmoothAnimator Create(double _value) => new(_value, Target);
        }

        public DoubleSmoothAnimator(double _value, double _target) : base(_value, _target)
        { }

        protected override double Smooth(double _current, double _target, double _alpha)
            => SmoothScalar(_current, _target, _alpha);

    }

}
