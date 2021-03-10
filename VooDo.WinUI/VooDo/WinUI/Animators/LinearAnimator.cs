using System;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class LinearAnimator<TValue> : NoOvershootTargetedAnimator<TValue>, IAnimatorWithVelocity<double> where TValue : notnull
    {

        public abstract record LinearFactory<TAnimator>(TValue Target, double Speed) : TargetedFactory<TAnimator>(Target) where TAnimator : LinearAnimator<TValue>
        {

            protected override void Set(TAnimator _animator, IController<TValue> _oldController)
            {
                base.Set(_animator, _oldController);
                _animator.Speed = Speed;
            }

        }

        protected LinearAnimator(TValue _value, TValue _target) : base(_value, _target)
        { }

        public const double defaultSpeed = 1;

        public double Speed { get; protected set; } = defaultSpeed;

        double IAnimatorWithVelocity<double>.Velocity => Speed;

        protected sealed override TValue Update(double _deltaTime, TValue _current, TValue _target)
            => Lerp(_current, _target, Math.Min(_deltaTime * defaultSpeed, 1));

        protected double LerpScalar(double _current, double _target, double _deltaTime)
        {
            double newValue = _current + Math.CopySign(_deltaTime * Speed, _target - _current);
            if ((_target > _current && newValue > _target) || (_target < _current && newValue < _target))
            {
                newValue = _target;
            }
            return newValue;
        }

        protected abstract TValue Lerp(TValue _current, TValue _target, double _deltaTime);

    }

    public sealed class DoubleLinearAnimator : LinearAnimator<double>
    {

        public sealed record DoubleLinearFactory(double Target, double SpeedFactor) : LinearFactory<DoubleLinearAnimator>(Target, SpeedFactor)
        {
            protected override DoubleLinearAnimator Create(double _value) => new(_value, Target);
        }

        public DoubleLinearAnimator(double _value, double _target) : base(_value, _target)
        { }

        protected override double Lerp(double _current, double _target, double _deltaTime)
            => LerpScalar(_current, _target, _deltaTime);

    }

}
