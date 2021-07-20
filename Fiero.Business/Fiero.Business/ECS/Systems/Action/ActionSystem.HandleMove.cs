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
        private bool HandleMove(ActorTime t, ref IAction action, ref int? cost)
        {
            var direction = default(Coord);
            if (action is MoveRelativeAction rel)
                direction = rel.Coord;
            else if (action is MoveRandomlyAction ran)
                direction = new(Rng.Random.Next(-1, 2), Rng.Random.Next(-1, 2));
            else if (action is MoveTowardsAction tow)
                direction = tow.Follow.Physics.Position - t.Actor.Physics.Position;
            else throw new NotSupportedException();

            var floorId = t.Actor.FloorId();
            var oldPos = t.Actor.Physics.Position;
            var newPos = t.Actor.Physics.Position + direction;
            if (newPos == t.Actor.Physics.Position) {
                action = new WaitAction();
                cost = HandleAction(t, ref action);
            }
            else if (_floorSystem.TryGetTileAt(floorId, newPos, out var tile)) {
                if (tile.TileProperties.Name == TileName.Door) {
                    _floorSystem.SetTileAt(t.Actor.FloorId(), newPos, TileName.Ground);
                    t.Actor.Log?.Write($"$Action.YouOpenThe$ {tile.TileProperties.Name}.");
                    // TODO: Move to event
                }
                else if (!tile.TileProperties.BlocksMovement) {
                    var actorsHere = _floorSystem.GetActorsAt(floorId, newPos);
                    var featuresHere = _floorSystem.GetFeaturesAt(floorId, newPos);
                    var itemsHere = _floorSystem.GetItemsAt(floorId, newPos);
                    if (!actorsHere.Any()) {
                        if (!featuresHere.Any(f => f.FeatureProperties.BlocksMovement)) {
                            return ActorMoved.Request(new(t.Actor, oldPos, newPos)).All(x => x);
                        }
                        else {
                            var feature = featuresHere.Single();
                            // you can bump shrines and chests to interact with them
                            action = new InteractWithFeatureAction(feature); 
                            cost = HandleAction(t, ref action);
                        }
                    }
                    else {
                        var target = actorsHere.Single();
                        if (t.Actor.IsHostileTowards(target)) {
                            // attack-bump is a free "combo"
                            action = new MeleeAttackOtherAction(target);
                            cost = HandleAction(t, ref action);
                        }
                        else if(t.Actor.IsFriendlyTowards(target)) {
                            // you can swap position with allies in twice the amount of time it takes to move
                            cost *= 2;
                            return ActorMoved.Request(new(t.Actor, oldPos, newPos)).Concat(
                                   ActorMoved.Request(new(target, newPos, oldPos))).All(x => x);
                        }
                    }
                }
                else {
                    t.Actor.Log?.Write("$Action.YouBumpIntoTheWall$.");
                    if (t.Actor.ActorProperties.Type == ActorName.Player) {
                        _sounds.Get(SoundName.WallBump).Play();
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
