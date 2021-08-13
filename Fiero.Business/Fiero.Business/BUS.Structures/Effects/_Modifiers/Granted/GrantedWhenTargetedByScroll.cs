using System;
using System.Linq;

namespace Fiero.Business
{
    public class GrantedWhenTargetedByScroll : ReadEffect
    {
        public readonly ScrollModifierName Modifier;

        public GrantedWhenTargetedByScroll(EffectDef source, ScrollModifierName modifier) : base(source)
        {
            Modifier = modifier;
        }

        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => "$Effect.GrantedWhenTargetedByScroll$";
        public override EffectName Type => Source.Name;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            if(Modifier == ScrollModifierName.Self) {
                Source.Resolve().Start(systems, target);
                return;
            }
            var floorId = target.FloorId();
            foreach (var p in target.Fov.VisibleTiles[floorId]) {
                var validTargets = systems.Floor.GetActorsAt(floorId, p)
                    .Where(a => Modifier switch {
                        ScrollModifierName.AreaAffectsAllies => systems.Faction.GetRelationships(target, a).Left.IsFriendly(),
                        ScrollModifierName.AreaAffectsEnemies => systems.Faction.GetRelationships(target, a).Left.IsHostile(),
                        ScrollModifierName.AreaAffectsEveryoneButTarget => a != target,
                        ScrollModifierName.AreaAffectsEveryone => true,
                        _ => throw new NotSupportedException(Modifier.ToString())
                    });
                foreach (var otherTarget in validTargets) {
                    Source.Resolve().Start(systems, otherTarget);
                }
            }
        }
    }
}
