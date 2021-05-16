using System;

namespace Fiero.Core
{
    public class Limiter : IEffect
    {
        public bool NextSample(int sr, float t, double sample, out double effectedSample)
        {
            effectedSample = sample;
            return true;
        }
    }
}
