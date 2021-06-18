using System;
using System.Numerics;

namespace Fiero.Business
{
    public readonly struct Relationship
    {
        public readonly StandingName Standing;
        public readonly TrustName Trust;
        public readonly PowerComparisonName Power;

        public Relationship With(StandingName s) => new(s, Trust, Power);
        public Relationship With(TrustName t) => new(Standing, t, Power);
        public Relationship With(PowerComparisonName p) => new(Standing, Trust, p);

        public Relationship(StandingName s, TrustName t, PowerComparisonName p)
        {
            Standing = s;
            Trust = t;
            Power = p;
        }

        public Vector3 ToVector() => new(
            (int)Standing / 3f,
            (int)Trust / 3f,
            (int)Power / 3f
        );

        public static Relationship FromVector(Vector3 v) => new(
            (StandingName)(int)(Math.Clamp(v.X * 3f, -3, 3)),
            (TrustName)(int)(Math.Clamp(v.Y * 3f, -3, 3)),
            (PowerComparisonName)(int)(Math.Clamp(v.Z * 3f, -3, 3))
        );
    }
}
