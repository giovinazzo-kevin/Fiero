using Fiero.Core;
using System;

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
        public readonly Script Script;

        public EffectDef(EffectName name, int magnitude = 1, int? duration = null, float? chance = null, bool canStack = false, Entity source = null, Script script = null)
        {
            Name = name;
            Magnitude = magnitude;
            Duration = duration;
            Chance = chance;
            Stacking = canStack;
            Source = source;
            Script = script;
        }

        public EffectDef AsPermanent() => new(Name, Magnitude, null, Chance, Stacking, Source, Script);
        public EffectDef AsCertain() => new(Name, Magnitude, Duration, null, Stacking, Source, Script);
        public EffectDef AsStacking() => new(Name, Magnitude, Duration, Chance, true, Source, Script);
        public EffectDef WithSource(Entity source) => new(Name, Magnitude, Duration, Chance, Stacking, source, Script);

        public static EffectDef FromScript(Script s) => new EffectDef(EffectName.Script, script: s);

        public Effect Resolve(Entity source)
        {
            source = source ?? Source;
            if (!Stacking)
            {
                return new NonStacking(WithSource(source).AsStacking());
            }
            if (Chance.HasValue)
            {
                return new Probabilistic(WithSource(source).AsCertain(), Chance.Value);
            }
            if (Duration.HasValue)
            {
                return new Temporary(WithSource(source).AsPermanent(), Duration.Value);
            }
            return Name switch
            {
                EffectName.Confusion => new ConfusionEffect(source),
                EffectName.Sleep => new SleepEffect(source),
                EffectName.Silence => new SilenceEffect(source),
                EffectName.Entrapment => new EntrapEffect(source),
                EffectName.Poison => new PoisonEffect(source, Magnitude * 2),
                EffectName.RaiseUndead => new RaiseUndeadEffect(source, (UndeadRaisingName)Magnitude),
                EffectName.UncontrolledTeleport => new UncontrolledTeleportEffect(source),
                EffectName.MagicMapping => new MagicMappingEffect(source),
                EffectName.Heal => new HealEffect(source, Magnitude * 10),
                EffectName.Explosion => new ExplosionEffect(source, Magnitude * 5, Shapes.Disc(Coord.Zero, Magnitude * 3 - 1)),
                EffectName.Trap => new TrapEffect(),
                EffectName.AutoPickup => new AutopickupEffect(),
                EffectName.IncreaseMaxMP => new IncreaseMaxMPEffect(source, Magnitude),
                EffectName.IncreaseMaxHP => new IncreaseMaxHPEffect(source, Magnitude),
                EffectName.Script when Script is null => throw new ArgumentNullException(nameof(Script)),
                EffectName.Script => new ScriptEffect(Script),
                _ => throw new NotSupportedException(Name.ToString()),
            };
        }

        public override string ToString() => $"{Name}";
    }
}
