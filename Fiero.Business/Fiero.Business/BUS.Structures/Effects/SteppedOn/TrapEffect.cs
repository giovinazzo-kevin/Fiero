using System;

namespace Fiero.Business
{
    public class TrapEffect : SteppedOnEffect
    {
        public override string Name => "$Trap.Name$";
        public override string Description => "$Trap.Desc$";
        public override EffectName Type => EffectName.Trap;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            if (owner.TryCast<Feature>(out var feature)) {
                systems.Action.ActorSteppedOnTrap.Raise(new(target, feature));
                // Removing the feature automatically ends all of its effects, so there's no need to call End()
                systems.Floor.RemoveFeature(feature);
            }
            else {
                End();
            }
        }

        protected override void OnRemoved(GameSystems systems, Entity owner, Actor target)
        {
            // do nothing
        }
    }
}
