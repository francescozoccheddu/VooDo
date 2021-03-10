using System;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class SpringAnimator<TValue> : TargetedAnimator<TValue> where TValue : notnull
    {

        public abstract record SpringFactory<TAnimator>(TValue Target, double Stiffness, double Damping, double Mass, double MinVelocity, double MinDifference) : TargetedFactory<TAnimator>(Target) where TAnimator : SpringAnimator<TValue>
        {

            protected override void Set(TAnimator _animator, IController<TValue> _oldController)
            {
                base.Set(_animator, _oldController);
                _animator.Stiffness = Stiffness;
                _animator.Damping = Damping;
                _animator.Mass = Mass;
                _animator.MinVelocity = MinVelocity;
                _animator.MinDifference = MinDifference;
            }

        }

        protected SpringAnimator(TValue _value, TValue _target) : base(_value, _target)
        { }

        public const double defaultStiffness = 100;
        public const double defaultDamping = 10;
        public const double defaultMass = 1;
        public const double defaultMinVelocity = double.Epsilon * 100;
        public const double defaultMinDifference = double.Epsilon * 100;

        public double Stiffness { get; protected set; } = defaultStiffness;
        public double Damping { get; protected set; } = defaultDamping;
        public double Mass { get; protected set; } = defaultMass;
        public double MinVelocity { get; protected set; } = defaultMinVelocity;
        public double MinDifference { get; protected set; } = defaultMinDifference;

        protected double SpringScalar(double _current, double _target, ref double _velocity, double _deltaTime)
        {
            double springForceY = -Stiffness * (_current - _target);
            double dampingForceY = Damping * _velocity;
            double forceY = springForceY - dampingForceY;
            double accelerationY = forceY / Mass;
            _velocity += accelerationY * _deltaTime;
            _current += _velocity * _deltaTime;
            if (Math.Abs(_velocity) < MinVelocity && Math.Abs(_current - _target) < MinDifference)
            {
                _velocity = 0;
                _current = _target;
            }
            return _current;
        }

    }

    public sealed class DoubleSpringAnimator : SpringAnimator<double>, IAnimatorWithVelocity<double>
    {

        public sealed record DoubleSpringFactory(double Target, double Stiffness, double Damping, double Mass, double MinVelocity, double MinDifference) : SpringFactory<DoubleSpringAnimator>(Target, Stiffness, Damping, Mass, MinVelocity, MinDifference)
        {
            protected override DoubleSpringAnimator Create(double _value) => new(_value, Target);
            protected override void Set(DoubleSpringAnimator _animator, IController<double> _oldController)
            {
                base.Set(_animator, _oldController);
                if (_oldController is IAnimatorWithVelocity<double> animator)
                {
                    _animator.Velocity = animator.Velocity;
                }
            }
        }

        public DoubleSpringAnimator(double _value, double _target) : base(_value, _target)
        { }

        public double Velocity { get; private set; }

        protected override bool Update(double _deltaTime)
        {
            double velocity = Velocity;
            double value = SpringScalar(Value, Target, ref velocity, _deltaTime);
            Velocity = velocity;
            SetValue(value);
            return Velocity != 0 || Value != Target;
        }

    }

    public interface IAnimatorWithVelocity<TVelocity> where TVelocity : notnull
    {

        TVelocity Velocity { get; }

    }

}
