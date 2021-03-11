using System;

namespace VooDo.WinUI.Animation
{

    public static class AnimationUtils
    {

        public static double Lerp(double _a, double _b, double _alpha)
            => (_a * (1 - _alpha)) + (_b * _alpha);

        public static double LerpClamped(double _a, double _b, double _alpha)
            => Lerp(_a, _b, Math.Clamp(_alpha, 0, 1));
    }

}
