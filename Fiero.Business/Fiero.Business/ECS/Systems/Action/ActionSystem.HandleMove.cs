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
        protected virtual bool HandleMove(Actor actor, ref IAction action, ref int? cost)
        {
            var direction = default(Coord);
            if (action is MoveRelativeAction rel)
                direction = rel.Coord;
            else if (action is MoveRandomlyAction ran)
                direction = new(Rng.Random.Next(-1, 2), Rng.Random.Next(-1, 2));
            else if (action is MoveTowardsAction tow)
                direction = tow.Follow.Physics.Position - actor.Physics.Position;
            else throw new NotSupportedException();

            var newPos = actor.Physics.Position + direction;
            if (newPos == actor.Physics.Position) {
                actor.ActorProperties.Health = Math.Min(actor.ActorProperties.MaximumHealth, actor.ActorProperties.Health + 1);
                action = new WaitAction();
                cost = HandleAction(actor, ref action);
                return true;
            }

            if (_floorSystem.TileAt(newPos, out var tile)) {
                if (tile.TileProperties.Name == TileName.Door) {
                    _floorSystem.UpdateTile(newPos, TileName.Ground);
                    actor.Log?.Write($"$Action.YouOpenThe$ {tile.TileProperties.Name}.");
                }
                else if (!tile.TileProperties.BlocksMovement) {
                    var actorsHere = _floorSystem.ActorsAt(newPos);
                    var featuresHere = _floorSystem.FeaturesAt(newPos);
                    var itemsHere = _floorSystem.ItemsAt(newPos);
                    if (!actorsHere.Any()) {
                        if (!featuresHere.Any(f => f.Properties.BlocksMovement)) {
                            actor.Physics.Position = newPos;
                            if (itemsHere.Any()) {
                                var item = itemsHere.Single();
                                actor.Log?.Write($"$Action.YouStepOverA$ {item.DisplayName}.");
                            }
                            else if (featuresHere.Any()) {
                                var feature = featuresHere.Single();
                                actor.Log?.Write($"$Action.YouStepOverA$ {feature.Info.Name}.");
                            }
                        }
                        else {
                            var feature = featuresHere.Single();
                            // you can bump shrines and chests to interact with them
                            action = new InteractWithFeatureAction(feature); 
                            cost = HandleAction(actor, ref action);
                            return true;
                        }
                    }
                    else {
                        var target = actorsHere.Single();
                        if (actor.IsHotileTowards(target)) {
                            // attack-bump is a free "combo"
                            action = new AttackOtherAction(target);
                            cost = HandleAction(actor, ref action);
                            return true;
                        }
                    }
                }
                else {
                    actor.Log?.Write("$Action.YouBumpIntoTheWall$.");
                    if (actor.ActorProperties.Type == ActorName.Player) {
                        _sounds.Get(SoundName.WallBump).Play();
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
