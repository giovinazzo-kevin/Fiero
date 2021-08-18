using Fiero.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public readonly struct EffectDef
    {
        public readonly EffectName Name;
        public readonly int Magnitude;
        public readonly int? Duration;
        public readonly float? Chance;
        public readonly bool Stacking;
        public readonly Entity Source;

        public EffectDef(EffectName name, int magnitude = 1, int? duration = null, float? chance = null, bool canStack = false, Entity source = null)
        {
            Name = name;
            Magnitude = magnitude;
            Duration = duration;
            Chance = chance;
            Stacking = canStack;
            Source = source;
        }

        public EffectDef AsNonTemporary() => new(Name, Magnitude, null, Chance, Stacking, Source);
        public EffectDef AsNonProbabilistic() => new(Name, Magnitude, Duration, null, Stacking, Source);
        public EffectDef AsStacking() => new(Name, Magnitude, Duration, Chance, true, Source);
        public EffectDef WithSource(Entity source) => new(Name, Magnitude, Duration, Chance, Stacking, source);

        public Effect Resolve(Entity source)
        {
            source = source ?? Source;
            if (!Stacking) {
                return new NonStacking(WithSource(source).AsStacking());
            }
            if (Chance.HasValue) {
                return new Chance(WithSource(source).AsNonProbabilistic(), Chance.Value);
            }
            if (Duration.HasValue) {
                return new Temporary(WithSource(source).AsNonTemporary(), Duration.Value);
            }
            return Name switch {
                EffectName.Confusion => new ConfusionEffect(source),
                EffectName.Sleep => new SleepEffect(source),
                EffectName.Silence => new SilenceEffect(source),
                EffectName.Entrapment => new EntrapEffect(source),
                EffectName.UncontrolledTeleport => new UncontrolledTeleportEffect(source),
                EffectName.Heal => new HealEffect(source, Magnitude * 10),
                EffectName.Explosion => new ExplosionEffect(source, Magnitude * 5, Shapes.Disc(Coord.Zero, Magnitude * 3 - 1)),
                EffectName.Trap => new TrapEffect(),
                EffectName.Autopickup => new AutopickupEffect(),
                _ => throw new NotSupportedException(Name.ToString()),
            };
        }

        public override string ToString() => $"{Name}";
    }
}
