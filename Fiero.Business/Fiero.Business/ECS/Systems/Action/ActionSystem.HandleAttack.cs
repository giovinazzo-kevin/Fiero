using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        protected virtual bool HandleAttack(Actor actor, ref IAction action, ref int? cost)
        {
            var victim = default(Actor);
            if (action is AttackOtherAction oth)
                victim = oth.Victim;
            else if (action is AttackDirectionAction dir) {
                var newPos = actor.Physics.Position + dir.Coord;
                var actorsHere = _floorSystem.ActorsAt(newPos);
                if (!actorsHere.Any(a => actor.Faction.Relationships.Get(a.Faction.Type).MayAttack())) {
                    return false;
                }
                victim = actorsHere.Single();
            }
            else throw new NotSupportedException(action.GetType().Name);
            if (actor.DistanceFrom(victim) >= 2) {
                // out of reach
                return false;
            }
            if (actor.Faction.Relationships.Get(victim.Faction.Type).MayAttack()) {
                // attack!
                actor.Log?.Write($"$Action.YouAttack$ {victim.Info.Name}.");
                victim.Log?.Write($"{actor.Info.Name} $Action.AttacksYou$.");
                // make sure that neutrals aggro the attacker
                if (victim.AI != null && victim.AI.Target == null) {
                    victim.AI.Target = actor;
                }
                // make sure that people hold a grudge regardless of factions
                victim.ActorProperties.Relationships.TryUpdate(actor, x => x
                    .With(StandingName.Hated)
                , out _);

                if (--victim.ActorProperties.Health <= 0) {
                    victim.Log?.Write($"{actor.Info.Name} $Action.KillsYou$.");
                    actor.Log?.Write($"$Action.YouKill$ {victim.Info.Name}.");
                    if (victim.ActorProperties.Type == ActorName.Player) {
                        _sounds.Get(SoundName.PlayerDeath).Play();
                        _store.SetValue(Data.Player.KilledBy, actor);
                    }
                    RemoveActor(victim.Id);
                    _floorSystem.CurrentFloor.RemoveActor(victim.Id);
                    _floorSystem.CurrentFloor.Entities.FlagEntityForRemoval(victim.Id);
                    victim.TryRefresh(0); // invalidate target proxy
                }
            }
            else {
                // friendly fire?
            }
            return true;
        }
    }
}
