using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public static class AnimatorFactory
    {

        public static IControllerFactory<double> Smooth(double _target)
        {
            return new SmoothAnimator.Factory(_target);
        }

    }

}
