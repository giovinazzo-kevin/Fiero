namespace Fiero.Business
{
    public class GrantedWhenHitByThrownItem : ThrowEffect
    {
        public GrantedWhenHitByThrownItem(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenHitByThrownItem$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Actor target)
        {
            Source.Resolve(source).Start(systems, target);
        }

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Coord location)
        {
            var target = systems.Get<DungeonSystem>().GetTileAt(source.FloorId(), location);
            Source.Resolve(source).Start(systems, target);
        }
    }
}
