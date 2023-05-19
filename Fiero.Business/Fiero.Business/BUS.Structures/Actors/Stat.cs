using System;

namespace Fiero.Business
{

    public class Stat<T>
        where T : IComparable<T>
    {
        private T _value;
        public T V
        {
            get => _value;
            set
            {
                if (value.CompareTo(Min) < 0)
                {
                    _value = Min;
                    ValueChanged?.Invoke(_value);
                }
                else if (value.CompareTo(Max) > 0)
                {
                    _value = Max;
                    ValueChanged?.Invoke(_value);
                }
                else
                {
                    _value = value;
                    ValueChanged?.Invoke(_value);
                }
            }
        }
        private T _min;
        public T Min
        {
            get => _min;
            set
            {
                _min = value;
                MinChanged?.Invoke(_value);
            }
        }
        private T _max;
        public T Max
        {
            get => _max;
            set
            {
                _max = value;
                MaxChanged?.Invoke(_value);
            }
        }

        public event Action<T> ValueChanged, MinChanged, MaxChanged;

        public Stat(T min, T max, T init, Action<T> valueChanged = null)
        {
            Min = min;
            Max = max;
            if (valueChanged != null)
            {
                ValueChanged += valueChanged;
            }
            V = init;
        }

        public static implicit operator T(Stat<T> k) => k.V;
        public override string ToString() => $"{{ {V} }}";
    }

    public class Stat : Stat<int>
    {
        public float Percentage => V / (float)Max;

        public Stat(int min, int max, int init, Action<int> valueChanged = null)
            : base(min, max, init, valueChanged)
        {

        }

        public static implicit operator int(Stat s) => s.V;
    }
}
