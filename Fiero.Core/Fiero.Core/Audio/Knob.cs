using System;

namespace Fiero.Core
{
    public class Knob<T>
        where T : IComparable<T>
    {
        public readonly T Min, Max;
        private T _value;
        public T V {
            get => _value;
            set {
                if(value.CompareTo(Min) < 0) {
                    _value = Min;
                    ValueChanged?.Invoke(_value);
                }
                else if(value.CompareTo(Max) > 0) {
                    _value = Max;
                    ValueChanged?.Invoke(_value);
                }
                else {
                    _value = value;
                    ValueChanged?.Invoke(_value);
                }
            }
        }

        public event Action<T> ValueChanged;

        public Knob(T min, T max, T init, Action<T> valueChanged = null)
        {
            Min = min;
            Max = max;
            if (valueChanged != null) {
                ValueChanged += valueChanged;
            }
            V = init;
        }

        public static implicit operator T(Knob<T> k) => k.V;

        public override string ToString() => $"{{ {V} }}";
    }
}
