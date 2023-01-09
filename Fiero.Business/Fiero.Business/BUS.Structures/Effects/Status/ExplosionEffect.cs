using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class ExplosionEffect : StatusEffect
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

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            // When an explosion is caused by an actor, it happens at the end of that actor's turn
            // When an explosion is caused by the environment and targets an actor, it happens at the end of that actor's turn
            // When an explosion is caused by the environment and targets no actor, it happens at the end of the turn
            var sourceIsActor = Source.TryCast<Actor>(out _);
            var ownerIsActor = owner.TryCast<Actor>(out _);
            if(sourceIsActor || ownerIsActor) {
                yield return systems.Action.ActorTurnEnded.SubscribeHandler(e => {
                    if (sourceIsActor && e.Actor != Source
                    || !sourceIsActor && e.Actor != owner)
                        return;
                    Inner();
                });
            }
            else {
                yield return systems.Action.TurnEnded.SubscribeHandler(e => {
                    Inner();
                });
            }

            void Inner()
            {
                if (!owner.TryCast<PhysicalEntity>(out var phys)) {
                    return;
                }
                var floorId = phys.FloorId();
                var pos = phys.Position();
                var actualShape = Shape
                    .Where(p => !Shapes.Line(pos, p + pos).Skip(1).Any(p => !systems.Dungeon.TryGetTileAt(floorId, p, out var t) || !t.IsWalkable(null)))
                    .ToArray();
                systems.Action.ExplosionHappened.HandleOrThrow(new(owner, pos, actualShape.Select(s => s + pos).ToArray(), BaseDamage));
                foreach (var p in actualShape) {
                    foreach (var a in systems.Dungeon.GetActorsAt(floorId, p + pos)) {
                        var damage = (int)(BaseDamage / (a.SquaredDistanceFrom(pos) + 1));
                        systems.Action.ActorDamaged.HandleOrThrow(new(owner, a, owner, damage));
                    }
                }
                End();
            }
        }

        protected override void Apply(GameSystems systems, Actor target) { }
    }
}
