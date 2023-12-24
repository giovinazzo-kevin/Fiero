using Unconcern.Common;

namespace Fiero.Business
{
    public class MagicMappingEffect : TypedEffect<Actor>
    {
        public MagicMappingEffect(Entity source) : base(source) { }
        public override string DisplayName => "$Effect.MagicMapping.Name$";
        public override string DisplayDescription => "$Effect.MagicMapping.Desc$";
        public override EffectName Name => EffectName.MagicMapping;

        protected override void TypedOnStarted(MetaSystem systems, Actor target)
        {
            var fid = target.FloorId();
            foreach (var tile in systems.Get<DungeonSystem>().GetAllTiles(fid))
                target.Fov.KnownTiles[fid].Add(tile.Position());
            systems.Get<ActionSystem>().ActorUsedMagicMapping.HandleOrThrow(new(target, Source));
            End(systems, target);
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
