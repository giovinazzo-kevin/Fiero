using System;

namespace Fiero.Business
{
    public readonly struct FloorConnection
    {
        public readonly FloorId From;
        public readonly FloorId To;

        public FloorConnection(FloorId a, FloorId b) => (From, To) = (a, b);

        public FloorConnection Reversed() => new(To, From);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(From);
            hash.Add(To);
            return hash.ToHashCode();
        }

        public override bool Equals(object obj) => obj is FloorConnection c
            ? c.From == From && c.To == To
            : base.Equals(obj);

        public static bool operator ==(FloorConnection left, FloorConnection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FloorConnection left, FloorConnection right)
        {
            return !(left == right);
        }
    }
}
