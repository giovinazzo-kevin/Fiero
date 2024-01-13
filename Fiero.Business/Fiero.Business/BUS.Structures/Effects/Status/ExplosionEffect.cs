using Unconcern.Common;

namespace Fiero.Business
{
    public class ExplosionEffect : TypedEffect<Actor>
    {
        public override string DisplayName => "$Effect.Explosion.Name$";
        public override string DisplayDescription => "$Effect.Explosion.Desc$";
        public override EffectName Name => EffectName.Explosion;

        public readonly int BaseDamage;
        public readonly Coord[] Shape;

        public ExplosionEffect(Entity source, int baseDamage, IEnumerable<Coord> shape)
            : base(source)
        {
            BaseDamage = baseDamage;
            Shape = shape.ToArray();
        }

        protected override void OnStarted(MetaSystem systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (!owner.TryCast<PhysicalEntity>(out var phys))
            {
                return;
            }
            var dungeon = systems.Get<DungeonSystem>();
            var action = systems.Get<ActionSystem>();
            var floorId = phys.FloorId();
            var pos = phys.Position();
            var actualShape = Shape
                .Where(p => !Shapes.Line(pos, p + pos).Skip(1).Any(p => !dungeon.TryGetTileAt(floorId, p, out var t) || !t.IsWalkable(phys)))
                .ToArray();
            action.ExplosionHappened.HandleOrThrow(new(Source, owner, floorId, pos, actualShape.Select(s => s + pos).ToArray(), BaseDamage));
            End(systems, owner);
        }
        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
