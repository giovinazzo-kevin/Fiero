namespace Fiero.Business
{
    public class GrantedWhenSteppedOn : SteppedOnEffect
    {
        public readonly bool IsTrap;
        public readonly bool AutoRemove;

        public GrantedWhenSteppedOn(EffectDef source, bool isTrap, bool autoRemove) : base(source)
        {
            IsTrap = isTrap;
            AutoRemove = autoRemove;
        }

        public override string DisplayName => "$Trap.Name$";
        public override string DisplayDescription => "$Trap.Desc$";
        public override EffectName Name => EffectName.Trap;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor target)
        {
            if (owner.TryCast<Feature>(out var feature))
            {
                if (IsTrap)
                {
                    systems.Get<ActionSystem>().ActorSteppedOnTrap.Raise(new(target, feature));
                }
                if (AutoRemove)
                {
                    Subscriptions.Add(systems.Get<ActionSystem>().TurnEnded.SubscribeHandler(e =>
                    {
                        // Removing the feature automatically ends all of its effects, so there's no need to call End()
                        systems.Get<DungeonSystem>().RemoveFeature(feature);
                    }));
                }
            }
            Source.Resolve(owner).Start(systems, target);
        }

        protected override void OnRemoved(MetaSystem systems, Entity owner, Actor target)
        {
            // do nothing
        }
    }
}
