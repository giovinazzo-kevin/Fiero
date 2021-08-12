namespace Fiero.Core
{

    public readonly struct OrderedPair<T1, T2>
    {
        public readonly T1 Left;
        public readonly T2 Right;

        public OrderedPair(T1 l, T2 r)
        {
            Left = l;
            Right = r;
        }

        public override int GetHashCode() => Left.GetHashCode() - Right.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is OrderedPair<T1, T2> other) {
                return Equals(other.Left, Left) && Equals(other.Right, Right);
            }
            return false;
        }

        public static bool operator ==(OrderedPair<T1, T2> left, OrderedPair<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrderedPair<T1, T2> left, OrderedPair<T1, T2> right)
        {
            return !(left == right);
        }

        public override string ToString() => $"({Left}, {Right})";
    }

    public readonly struct OrderedPair<T>
    {
        public readonly T Left;
        public readonly T Right;

        public OrderedPair(T l, T r)
        {
            Left = l;
            Right = r;
        }

        public override int GetHashCode() => Left.GetHashCode() - Right.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is OrderedPair<T> other) {
                return Equals(other.Left, Left) && Equals(other.Right, Right);
            }
            return false;
        }

        public static bool operator ==(OrderedPair<T> left, OrderedPair<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrderedPair<T> left, OrderedPair<T> right)
        {
            return !(left == right);
        }

        public override string ToString() => $"({Left}, {Right})";
    }
}
