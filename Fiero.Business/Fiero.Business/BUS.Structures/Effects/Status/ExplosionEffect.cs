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
            yield return systems.Action.ActorTurnEnded.SubscribeHandler(e => {
                if (e.Actor != Source)
                    return;
                if(!owner.TryCast<PhysicalEntity>(out var phys)) {
                    return;
                }
                var floorId = phys.FloorId();
                var pos = phys.Position();
                systems.Action.ExplosionHappened.HandleOrThrow(new(owner, pos, Shape.Select(s => s + pos).ToArray(), BaseDamage));
                foreach (var p in Shape) {
                    foreach (var a in systems.Floor.GetActorsAt(floorId, p + pos)) {
                        var damage = (int)(BaseDamage / (a.SquaredDistanceFrom(pos) + 1));
                        systems.Action.ActorDamaged.HandleOrThrow(new(owner, a, owner, damage));
                    }
                }
                End();
            });
        }

        protected override void Apply(GameSystems systems, Actor target) { }
    }
}
