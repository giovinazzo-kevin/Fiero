using Fiero.Core;
using System;
using System.Numerics;

namespace Fiero.Business
{
    public readonly struct Personality
    {
        public readonly EgoismName Egoism;
        public readonly GregariousnessName Gregariousness;
        public readonly ImpulsivityName Impulsivity;

        public Personality With(EgoismName s) => new(s, Gregariousness, Impulsivity);
        public Personality With(GregariousnessName t) => new(Egoism, t, Impulsivity);
        public Personality With(ImpulsivityName p) => new(Egoism, Gregariousness, p);

        public static Personality RandomPersonality()
        {
            var rng = new Random();
            return new(
                (EgoismName)rng.Next(-3, 4),
                (GregariousnessName)rng.Next(-3, 4),
                (ImpulsivityName)rng.Next(-3, 4)
            );
        }

        public Personality(EgoismName ego, GregariousnessName greg, ImpulsivityName imp)
        {
            Egoism = ego;
            Gregariousness = greg;
            Impulsivity = imp;
        }

        public Vector3 ToVector() => new(
            (int)Egoism / 3f,
            (int)Gregariousness / 3f,
            (int)Impulsivity / 3f
        );

        public static Personality FromVector(Vector3 v) => new(
            (EgoismName)(int)(Math.Clamp(v.X * 3f, -3, 3)),
            (GregariousnessName)(int)(Math.Clamp(v.Y * 3f, -3, 3)),
            (ImpulsivityName)(int)(Math.Clamp(v.Z * 3f, -3, 3))
        );
    }
}
