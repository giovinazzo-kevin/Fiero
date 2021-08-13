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

        public EffectDef(EffectName name, int? duration = null, float? chance = null)
        {
            Name = name;
            Duration = duration;
            Chance = chance;
        }

        public EffectDef NotTemporary() => new(Name, null, Chance);
        public EffectDef NotProbabilistic() => new(Name, Duration, null);

        public Effect Resolve()
        {
            if (Chance.HasValue) {
                return new Chance(NotProbabilistic(), Chance.Value);
            }
            if (Duration.HasValue) {
                return new Temporary(NotTemporary(), Duration.Value);
            }
            return Name switch {
                EffectName.Confusion => new ConfusionEffect(),
                EffectName.Sleep => throw new NotImplementedException(),
                EffectName.Silence => throw new NotImplementedException(),
                EffectName.Paralysis => throw new NotImplementedException(),
                EffectName.Bleeding => throw new NotImplementedException(),
                EffectName.Poison => throw new NotImplementedException(),
                EffectName.Berserk => throw new NotImplementedException(),
                EffectName.Hardening => throw new NotImplementedException(),
                EffectName.Evasion => throw new NotImplementedException(),
                EffectName.Vampirism => throw new NotImplementedException(),
                EffectName.Rabies => throw new NotImplementedException(),
                EffectName.Trap => new TrapEffect(),
                EffectName.Autopickup => new AutopickupEffect(),
                _ => throw new NotSupportedException(Name.ToString()),
            };
        }

        public override string ToString() => $"{Name}";
    }
}
