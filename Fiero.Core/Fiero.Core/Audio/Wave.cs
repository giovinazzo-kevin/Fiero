using System;

namespace Fiero.Core
{

    public static class Wave
    {
        public static double Sine(float t, float a, float f)
        {
            return a * Math.Sin(2 * Math.PI * f * t);
        }

        public static double Triangle(float t, float a, float f)
        {
            return (2 * a / Math.PI) * Math.Asin(Math.Sin(2 * Math.PI / 2 * f * t));
        }

        public static double Saw(float t, float a, float f)
        {
            return (2 * a) / Math.PI * Math.Atan(1f / Math.Tan(f * t * Math.PI) / 2);
        }

        public static double Square(float t, float a, float f)
        {
            return Math.Sign(Sine(t, a, f));
        }
    }
}
