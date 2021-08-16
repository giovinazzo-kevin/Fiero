﻿using Fiero.Core;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class UncontrolledTeleportEffect : StatusEffect
    {
        public override string DisplayName => "$Effect.UncontrolledTeleport.Name$";
        public override string DisplayDescription => "$Effect.UncontrolledTeleport.Desc$";
        public override EffectName Name => EffectName.UncontrolledTeleport;

        protected override void Apply(GameSystems systems, Actor target) 
        {
            var randomPos = systems.Floor.GetFloor(target.FloorId())
                .Cells.Shuffle(Rng.Random)
                .First(x => x.Value.IsWalkable(null))
                .Key;
            systems.Action.ActorTeleporting.HandleOrThrow(new(target, target.Position(), randomPos));
            End();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}