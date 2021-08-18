using Fiero.Core;
using System;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{

    // Heal a fixed amount of HP 
    public class HealEffect : StatusEffect
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

        protected override void Apply(GameSystems systems, Actor target)
        {
            systems.Action.ActorHealed.HandleOrThrow(new(target, target, target, Amount));
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
