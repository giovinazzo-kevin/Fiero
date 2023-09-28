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

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (!owner.TryCast<PhysicalEntity>(out var phys))
            {
                return;
            }
            var floorId = phys.FloorId();
            var pos = phys.Position();
            var actualShape = Shape
                .Where(p => !Shapes.Line(pos, p + pos).Skip(1).Any(p => !systems.Dungeon.TryGetTileAt(floorId, p, out var t) || !t.IsWalkable(phys)))
                .ToArray();
            systems.Action.ExplosionHappened.HandleOrThrow(new(owner, pos, actualShape.Select(s => s + pos).ToArray(), BaseDamage));
            // TODO: Make this a handelr of ExplosionHappened?
            foreach (var p in actualShape)
            {
                foreach (var a in systems.Dungeon.GetActorsAt(floorId, p + pos))
                {
                    var damage = (int)(BaseDamage / (a.SquaredDistanceFrom(pos) + 1));
                    systems.Action.ActorDamaged.HandleOrThrow(new(Source, a, new[] { owner }, damage));
                }
            }
            End(systems, owner);
        }
        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
