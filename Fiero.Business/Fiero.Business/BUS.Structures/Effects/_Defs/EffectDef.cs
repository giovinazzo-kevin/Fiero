using Ergo.Lang.Extensions;

namespace Fiero.Business
{
    public readonly struct EffectDef
    {
        public readonly EffectName Name;
        public readonly string Arguments;
        public readonly int? Duration;
        public readonly float? Chance;
        public readonly bool Stacking;
        public readonly Entity Source;
        public readonly Script Script;

        public EffectDef(EffectName name, string arguments = null, int? duration = null, float? chance = null, bool canStack = true, Entity source = null, Script script = null)
        {
            Name = name;
            Arguments = arguments;
            Duration = duration;
            Chance = chance;
            Stacking = canStack;
            Source = source;
            Script = script;
        }

        public EffectDef AsPermanent() => new(Name, Arguments, null, Chance, Stacking, Source, Script);
        public EffectDef AsCertain() => new(Name, Arguments, Duration, null, Stacking, Source, Script);
        public EffectDef AsStacking() => new(Name, Arguments, Duration, Chance, true, Source, Script);
        public EffectDef WithSource(Entity source) => new(Name, Arguments, Duration, Chance, Stacking, source, Script);

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
                EffectName.Invulnerable => new InvulnerableEffect(source),
                EffectName.Impassible => new ImpassibleEffect(source),
                EffectName.Entrapment => new EntrapEffect(source),
                EffectName.Poison => new PoisonEffect(source, int.Parse(Arguments) * 2),
                EffectName.RaiseUndead => new RaiseUndeadEffect(source, Enum.Parse<UndeadRaisingName>(Arguments.ToCSharpCase())),
                EffectName.UncontrolledTeleport => new UncontrolledTeleportEffect(source),
                EffectName.MagicMapping => new MagicMappingEffect(source),
                EffectName.Heal => new HealEffect(source, int.Parse(Arguments) * 10),
                EffectName.Vampirism => new VampirismEffect(this, int.Parse(Arguments)),
                EffectName.Explosion => new ExplosionEffect(source, int.Parse(Arguments), Shapes.Disc(Coord.Zero, float.Parse(Arguments))),
                EffectName.Trap => new TrapEffect(),
                EffectName.AutoPickup => new AutopickupEffect(),
                EffectName.IncreaseMaxMP => new IncreaseMaxMPEffect(source, int.Parse(Arguments)),
                EffectName.IncreaseMaxHP => new IncreaseMaxHPEffect(source, int.Parse(Arguments)),
                EffectName.BestowTrait => new BestowTraitEffect(source, Traits.Get(Enum.Parse<TraitName>(Arguments.ToCSharpCase()))),
                EffectName.RemoveTrait => new RemoveTraitEffect(source, Traits.Get(Enum.Parse<TraitName>(Arguments.ToCSharpCase()))),
                EffectName.Script when Script is null => throw new ArgumentNullException(nameof(Script)),
                EffectName.Script when Script.ScriptProperties.LastError == null => new ScriptEffect((Script)Script.Clone(), Arguments.ToErgoCase()),
                EffectName.Script when Script.ScriptProperties.LastError != null => new NullEffect(),
                EffectName.None => new NullEffect(),
                _ => throw new NotSupportedException(Name.ToString()),
            };
        }

        public override string ToString() => $"{Name}";
    }
}
