using System;

namespace VooDo.WinUI.Animation
{

    public static class AnimationUtils
    {

        public static double Lerp(double _a, double _b, double _alpha)
            => (_a * (1 - _alpha)) + (_b * _alpha);

        public static double LerpClamped(double _a, double _b, double _alpha)
            => Lerp(_a, _b, Math.Clamp(_alpha, 0, 1));

        public static class Quadratic
        {

            public static double In(double _k)
            {
                return _k * _k;
            }

            public static double Out(double _k)
            {
                return _k * (2f - _k);
            }

            public static double InOut(double _k)
            {
                if ((_k *= 2f) < 1f)
                {
                    return 0.5f * _k * _k;
                }

                return -0.5f * ((_k -= 1f) * (_k - 2f) - 1f);
            }

        };

        public static class Cubic
        {
            public static double In(double _k)
            {
                return _k * _k * _k;
            }

            public static double Out(double _k)
            {
                return 1f + ((_k -= 1f) * _k * _k);
            }

            public static double InOut(double _k)
            {
                if ((_k *= 2f) < 1f)
                {
                    return 0.5f * _k * _k * _k;
                }

                return 0.5f * ((_k -= 2f) * _k * _k + 2f);
            }
        };

        public static class Quartic
        {
            public static double In(double _k)
            {
                return _k * _k * _k * _k;
            }

            public static double Out(double _k)
            {
                return 1f - ((_k -= 1f) * _k * _k * _k);
            }

            public static double InOut(double _k)
            {
                if ((_k *= 2f) < 1f)
                {
                    return 0.5f * _k * _k * _k * _k;
                }

                return -0.5f * ((_k -= 2f) * _k * _k * _k - 2f);
            }
        };

        public static class Quintic
        {
            public static double In(double _k)
            {
                return _k * _k * _k * _k * _k;
            }

            public static double Out(double _k)
            {
                return 1f + ((_k -= 1f) * _k * _k * _k * _k);
            }

            public static double InOut(double _k)
            {
                if ((_k *= 2f) < 1f)
                {
                    return 0.5f * _k * _k * _k * _k * _k;
                }

                return 0.5f * ((_k -= 2f) * _k * _k * _k * _k + 2f);
            }
        };

    }

}
