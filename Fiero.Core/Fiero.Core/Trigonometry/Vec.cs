using SFML.System;
using System.Drawing;

namespace Fiero.Core
{
    public readonly struct Vec
    {
        public readonly float X, Y;
        public readonly double Magnitude() => Math.Sqrt(X * X + Y * Y);
        public Vec(float x = 0, float y = 0)
        {
            X = x;
            Y = y;
        }

        public static Vec operator +(Vec self, Vec other)
            => new Vec(self.X + other.X, self.Y + other.Y);
        public static Vec operator -(Vec self, Vec other)
            => new Vec(self.X - other.X, self.Y - other.Y);
        public static Vec operator *(Vec self, Vec other)
            => new Vec(self.X * other.X, self.Y * other.Y);
        public static Vec operator /(Vec self, Vec other)
            => new Vec(self.X / other.X, self.Y / other.Y);
        public static Vec operator +(Vec self, Coord other)
            => new Vec(self.X + other.X, self.Y + other.Y);
        public static Vec operator -(Vec self, Coord other)
            => new Vec(self.X - other.X, self.Y - other.Y);
        public static Vec operator *(Vec self, Coord other)
            => new Vec(self.X * other.X, self.Y * other.Y);
        public static Vec operator /(Vec self, Coord other)
            => new Vec(self.X / other.X, self.Y / other.Y);
        public static Vec operator +(Vec self, float other)
            => new Vec(self.X + other, self.Y + other);
        public static Vec operator -(Vec self, float other)
            => new Vec(self.X - other, self.Y - other);
        public static Vec operator *(Vec self, float other)
            => new Vec(self.X * other, self.Y * other);
        public static Vec operator /(Vec self, float other)
            => new Vec(self.X / other, self.Y / other);
        public static bool operator ==(Vec self, Vec other)
            => self.X == other.X && self.Y == other.Y;
        public static bool operator !=(Vec self, Vec other)
            => self.X != other.X || self.Y != other.Y;

        public void Deconstruct(out float x, out float y)
        {
            x = X; y = Y;
        }

        public static Vec Zero { get; } = new Vec(0, 0);


        public override bool Equals(object obj)
            => obj is Vec other && this == other;

        public static implicit operator Vector2f(Vec v) => new(v.X, v.Y);
        public static implicit operator Vector2i(Vec v) => new((int)v.X, (int)v.Y);
        public static implicit operator Vector2u(Vec v) => new((uint)v.X, (uint)v.Y);
        public static implicit operator Point(Vec v) => new((int)v.X, (int)v.Y);
        public Vec Clamp(float min = float.MinValue, float max = float.MaxValue)
            => new(Math.Clamp(X, min, max), Math.Clamp(Y, min, max));
        public Vec Clamp(float minX = float.MinValue, float maxX = float.MaxValue, float minY = float.MinValue, float maxY = float.MaxValue)
            => new(Math.Clamp(X, minX, maxX), Math.Clamp(Y, minY, maxY));
        public Vec Round(int places = 0)
            => new((float)Math.Round(X, places), (float)Math.Round(Y, places));
        public Vec Floor()
            => this.ToCoord().ToVec();
        public Vec Ceiling()
            => new((int)Math.Ceiling(X), (int)Math.Ceiling(Y));
        public Vec Abs()
            => new(Math.Abs(X), Math.Abs(Y));


        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(X);
            hash.Add(Y);
            return hash.ToHashCode();
        }

        public override string ToString() => $"{{ {X}; {Y} }}";

    }
}
