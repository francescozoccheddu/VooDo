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

    }

}
