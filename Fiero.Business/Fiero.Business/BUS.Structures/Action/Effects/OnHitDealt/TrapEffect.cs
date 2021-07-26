using System;

namespace Fiero.Business
{
    public class TrapEffect : SteppedOnEffect
    {
        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Console.WriteLine($"{owner} stepped on by {target}");

            target.Log?.Write("You step on a trap! Thankfully they're not implemented yet");
            if (owner.TryCast<Feature>(out var feature)) {
                // Removing the feature automatically ends all of its effects, so there's no need to call End()
                systems.Floor.RemoveFeature(feature.FloorId(), feature);
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
