using System;

namespace Fiero.Core
{
    public class Knob<T>
        where T : IComparable<T>
    {
        private readonly T _min, _max;
        private T _value;
        public T V {
            get => _value;
            set {
                if(value.CompareTo(_min) < 0) {
                    _value = _min;
                    ValueChanged?.Invoke(_value);
                }
                else if(value.CompareTo(_max) > 0) {
                    _value = _max;
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
            _min = min;
            _max = max;
            if (valueChanged != null) {
                ValueChanged += valueChanged;
            }
            V = init;
        }

        public static implicit operator T(Knob<T> k) => k.V;

        public override string ToString() => $"{{ {V} }}";
    }
}
