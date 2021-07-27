using Fiero.Core;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{

    /// <summary>
    /// Walking or waiting on a blood puddle heals you a little and drains the puddle a little
    /// </summary>
    public class BloodbathEffect : IntrinsicEffect
    {
        public override string Name => "$Spell.Bloodbath.Name$";
        public override string Description => "$Spell.Bloodbath.Desc$";

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Subscriptions.Add(systems.Action.ActorMoved.SubscribeHandler(e => {
                if (e.Actor == target)
                    Handle();
            }));
            Subscriptions.Add(systems.Action.ActorWaited.SubscribeHandler(e => {
                if (e.Actor == target)
                    Handle();
            }));

            void Handle()
            {
                var isFullhealth = target.ActorProperties.Stats.Health == target.ActorProperties.Stats.MaximumHealth;
                if (isFullhealth)
                    return;
                var splatterHere = systems.Floor.GetFeaturesAt(target.FloorId(), target.Position())
                    .TrySelect(f => (f.TryCast<BloodSplatter>(out var splatter), splatter))
                    .SingleOrDefault();
                if (splatterHere is null || !splatterHere.Blood.TryRemove(10))
                    return;
                if (splatterHere.Blood.Amount == 0) {
                    systems.Floor.RemoveFeature(splatterHere);
                }
                target.Log?.Write("$Spell.Bloodbath.ProcMessage$");
            }
        }

        protected override void OnRemoved(GameSystems systems, Entity owner, Actor target)
        {

        }
    }
}
