﻿using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{

    // Heal a fixed amount of HP 
    public class HealEffect : TypedEffect<Actor>
    {
        public override string DisplayName => "$Effect.Heal.Name$";
        public override string DisplayDescription => "$Effect.Heal.Desc$";
        public override EffectName Name => EffectName.Heal;
        public readonly int Amount;

        public HealEffect(Entity source, int amount)
            : base(source)
        {
            Amount = amount;
        }

        protected override void TypedOnStarted(GameSystems systems, Actor target)
        {
            systems.Action.ActorHealed.HandleOrThrow(new(target, target, target, Amount));
            End(systems, target);
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
