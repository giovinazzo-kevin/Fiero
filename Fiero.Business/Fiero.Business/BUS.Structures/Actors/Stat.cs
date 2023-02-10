using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class Stat : Knob<int>
    {
        public float Percentage => V / (float)Max;

        public Stat(int min, int max, int init, Action<int> valueChanged = null)
            : base(min, max, init, valueChanged)
        {

        }

        public static implicit operator int(Stat s) => s.V;
    }
}
