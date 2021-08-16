using System;
using System.Collections;
using System.Collections.Generic;

namespace Fiero.Business
{
    public readonly struct EffectDef
    {
        public readonly EffectName Name;
        public readonly int? Duration;
        public readonly float? Chance;
        public readonly bool Stacking;

        public EffectDef(EffectName name, int? duration = null, float? chance = null, bool canStack = false)
        {
            Name = name;
            Duration = duration;
            Chance = chance;
            Stacking = canStack;
        }

        public EffectDef AsNonTemporary() => new(Name, null, Chance, Stacking);
        public EffectDef AsNonProbabilistic() => new(Name, Duration, null, Stacking);
        public EffectDef AsStacking() => new(Name, Duration, Chance, true);

        public Effect Resolve()
        {
            if (!Stacking) {
                return new NonStacking(AsStacking());
            }
            if (Chance.HasValue) {
                return new Chance(AsNonProbabilistic(), Chance.Value);
            }
            if (Duration.HasValue) {
                return new Temporary(AsNonTemporary(), Duration.Value);
            }
            return Name switch {
                EffectName.Confusion => new ConfusionEffect(),
                EffectName.Sleep => new SleepEffect(),
                EffectName.Silence => new SilenceEffect(),
                EffectName.Entrapment => new EntrapEffect(),
                EffectName.Bleed => new BleedEffect(),
                EffectName.Poison => throw new NotImplementedException(),
                EffectName.Berserk => throw new NotImplementedException(),
                EffectName.Hardening => throw new NotImplementedException(),
                EffectName.Evasion => throw new NotImplementedException(),
                EffectName.Vampirism => throw new NotImplementedException(),
                EffectName.UncontrolledTeleport => new UncontrolledTeleportEffect(),
                EffectName.Trap => new TrapEffect(),
                EffectName.Autopickup => new AutopickupEffect(),
                _ => throw new NotSupportedException(Name.ToString()),
            };
        }

        public override string ToString() => $"{Name}";
    }
}
