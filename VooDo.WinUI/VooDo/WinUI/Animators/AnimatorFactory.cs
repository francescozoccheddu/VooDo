using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public static class AnimatorFactory
    {

        public static IControllerFactory<double> Smooth(
            double _target,
            double _speedFactor = DoubleSmoothAnimator.defaultSpeedFactor,
            double _minDifference = DoubleSmoothAnimator.defaultMinDifference)
            => new DoubleSmoothAnimator.DoubleSmoothFactory(_target, _speedFactor, _minDifference);

        public static IControllerFactory<double> Linear(
            double _target,
            double _speed = DoubleLinearAnimator.defaultSpeed)
            => new DoubleLinearAnimator.DoubleLinearFactory(_target, _speed);

        public static IControllerFactory<double> Spring(
            double _target,
            double _stiffness = DoubleSpringAnimator.defaultStiffness,
            double _damping = DoubleSpringAnimator.defaultDamping,
            double _mass = DoubleSpringAnimator.defaultMass,
            double _minVelocity = DoubleSpringAnimator.defaultMinVelocity)
            => new DoubleSpringAnimator.DoubleSpringFactory(_target, _stiffness, _damping, _mass, _minVelocity);

    }

}
