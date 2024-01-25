namespace Fiero.Business
{
    public class GrantedWhenHitByZappedWand : ZapEffect
    {
        public GrantedWhenHitByZappedWand(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenHitByZappedWand$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Actor target)
        {
            Source.Resolve(source).Start(systems, target, source);
        }

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Coord location)
        {
            var target = systems.Get<DungeonSystem>().GetTileAt(source.FloorId(), location);
            Source.Resolve(source).Start(systems, target, source);
        }
    }
}
