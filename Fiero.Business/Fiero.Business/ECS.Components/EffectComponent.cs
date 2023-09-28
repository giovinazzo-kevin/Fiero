namespace Fiero.Business
{
    public class EffectsComponent : EcsComponent
    {
        public bool Lock { get; set; }

        public readonly HashSet<EffectDef> Intrinsic = new();
        public readonly HashSet<Effect> Active = new();
    }
}
