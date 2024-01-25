namespace Fiero.Business
{
    public class GrantedWhenTargetedByScroll : ReadEffect
    {
        public readonly ScrollModifierName Modifier;

        public GrantedWhenTargetedByScroll(EffectDef source, ScrollModifierName modifier) : base(source)
        {
            Modifier = modifier;
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenTargetedByScroll$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor target)
        {
            if (Modifier == ScrollModifierName.Self)
            {
                Source.Resolve(target).Start(systems, target, owner);
                return;
            }
            var floorId = target.FloorId();
            foreach (var p in target.Fov.VisibleTiles[floorId])
            {
                var validTargets = Modifier switch
                {
                    ScrollModifierName.AreaAffectsItems => systems.Get<DungeonSystem>().GetItemsAt(floorId, p)
                        .Cast<Entity>(),
                    _ => systems.Get<DungeonSystem>().GetActorsAt(floorId, p)
                        .Where(a => Modifier switch
                        {
                            ScrollModifierName.AreaAffectsAllies => systems.Get<FactionSystem>().GetRelations(target, a).Left.IsFriendly(),
                            ScrollModifierName.AreaAffectsEnemies => systems.Get<FactionSystem>().GetRelations(target, a).Left.IsHostile(),
                            ScrollModifierName.AreaAffectsEveryoneButTarget => a != target,
                            ScrollModifierName.AreaAffectsEveryone => true,
                            _ => throw new NotSupportedException(Modifier.ToString())
                        })
                };

                foreach (var otherTarget in validTargets)
                {
                    Source.Resolve(target).Start(systems, otherTarget, owner);
                }
            }
        }
    }
}
