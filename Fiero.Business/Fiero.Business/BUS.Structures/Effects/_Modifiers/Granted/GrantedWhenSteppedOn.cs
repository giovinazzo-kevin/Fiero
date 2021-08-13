﻿using System;

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

        public override string Name => "$Trap.Name$";
        public override string Description => "$Trap.Desc$";
        public override EffectName Type => EffectName.Trap;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            if (owner.TryCast<Feature>(out var feature)) {
                if(IsTrap) {
                    systems.Action.ActorSteppedOnTrap.Raise(new(target, feature));
                }
                if(AutoRemove) {
                    // Removing the feature automatically ends all of its effects, so there's no need to call End()
                    systems.Floor.RemoveFeature(feature);
                }
            }
            Source.Resolve().Start(systems, target);
        }

        protected override void OnRemoved(GameSystems systems, Entity owner, Actor target)
        {
            // do nothing
        }
    }
}
