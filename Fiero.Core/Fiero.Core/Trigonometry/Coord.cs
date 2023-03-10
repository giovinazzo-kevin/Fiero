using SFML.System;
using System;
using System.Drawing;

namespace Fiero.Core
{
    public readonly struct Coord : IComparable<Coord>
    {
        public readonly int X, Y;
        public Coord(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public static Coord operator +(Coord self, Coord other)
            => new Coord(self.X + other.X, self.Y + other.Y);
        public static Coord operator -(Coord self, Coord other)
            => new Coord(self.X - other.X, self.Y - other.Y);
        public static Coord operator *(Coord self, Coord other)
            => new Coord(self.X * other.X, self.Y * other.Y);
        public static Coord operator /(Coord self, Coord other)
            => new Coord(self.X / other.X, self.Y / other.Y);
        public static Vec operator +(Coord self, Vec other)
            => new Vec(self.X + other.X, self.Y + other.Y);
        public static Vec operator -(Coord self, Vec other)
            => new Vec(self.X - other.X, self.Y - other.Y);
        public static Vec operator *(Coord self, Vec other)
            => new Vec(self.X * other.X, self.Y * other.Y);
        public static Vec operator /(Coord self, Vec other)
            => new Vec(self.X / other.X, self.Y / other.Y);
        public static Coord operator +(Coord self, int other)
            => new Coord(self.X + other, self.Y + other);
        public static Coord operator -(Coord self, int other)
            => new Coord(self.X - other, self.Y - other);
        public static Coord operator *(Coord self, int other)
            => new Coord(self.X * other, self.Y * other);
        public static Coord operator /(Coord self, int other)
            => new Coord(self.X / other, self.Y / other);
        public static Vector2f operator /(Coord self, float other)
            => new Vector2f(self.X / other, self.Y / other);
        public static Vector2f operator *(Coord self, float other)
            => new Vector2f(self.X * other, self.Y * other);
        public static bool operator ==(Coord self, Coord other)
            => self.X == other.X && self.Y == other.Y;
        public static bool operator !=(Coord self, Coord other)
            => self.X != other.X || self.Y != other.Y;

        public void Deconstruct(out int x, out int y)
        {
            x = X; y = Y;
        }

        public int ToIndex(int w)
        {
            return X + Y * w;
        }

        public static Coord FromIndex(int i, int w)
        {
            return new Coord(i % w, i / w);
        }

        public static Coord Zero { get; } = new Coord(0, 0);
        public static Coord PositiveOne { get; } = new Coord(+1, +1);
        public static Coord PositiveX { get; } = new Coord(+1, +0);
        public static Coord PositiveY { get; } = new Coord(+0, +1);
        public static Coord NegativeX { get; } = new Coord(-1, +0);
        public static Coord NegativeY { get; } = new Coord(+0, -1);
        public static Coord NegativeOne { get; } = new Coord(-1, -1);

        public override bool Equals(object obj)
            => obj is Coord other && this == other;

        public static implicit operator Vector2f(Coord v) => new(v.X, v.Y);
        public static implicit operator Vector2i(Coord v) => new(v.X, v.Y);
        public static implicit operator Vector2u(Coord v) => new((uint)v.X, (uint)v.Y);
        public static implicit operator Point(Coord v) => new(v.X, v.Y);

        public Coord Clamp(int min = int.MinValue, int max = int.MaxValue) => new(Math.Clamp(X, min, max), Math.Clamp(Y, min, max));

        public Coord Clamp(int minX = int.MinValue, int maxX = int.MaxValue, int minY = int.MinValue, int maxY = int.MaxValue)
            => new(Math.Clamp(X, minX, maxX), Math.Clamp(Y, minY, maxY));

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(X);
            hash.Add(Y);
            return hash.ToHashCode();
        }

        public override string ToString() => $"{{ {X}; {Y} }}";
        public int CompareTo(Coord other) => (X + Y).CompareTo(other.X + other.Y);
    }
}
