namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleMove(ActorTime t, ref IAction action, ref int? cost)
        {
            if (t.Actor.IsImmobile())
            {
                action = new WaitAction();
                cost = HandleAction(t, ref action);
                return true;
            }
            var direction = default(Coord);
            if (action is MoveRelativeAction rel)
                direction = rel.Coord;
            else if (action is MoveRandomlyAction ran)
                direction = new(Rng.Random.Next(-1, 2), Rng.Random.Next(-1, 2));
            else throw new NotSupportedException();

            var floorId = t.Actor.FloorId();
            var oldPos = t.Actor.Position();
            var newPos = t.Actor.Position() + direction;
            if (newPos == t.Actor.Position())
            {
                action = new WaitAction();
                cost = HandleAction(t, ref action);
            }
            else if (_floorSystem.TryGetCellAt(floorId, newPos, out var cell))
            {
                if (cell.Tile.IsWalkable(t.Actor))
                {
                    var actorsHere = _floorSystem.GetActorsAt(floorId, newPos);
                    var featuresHere = _floorSystem.GetFeaturesAt(floorId, newPos);
                    if (!actorsHere.Any(x => x.ActorProperties.Type != ActorName.None || x.Physics.BlocksMovement))
                    {
                        if (t.Actor.Physics.Phasing || !featuresHere.Any(f => f.Physics.BlocksMovement))
                        {
                            // Movement successful
                            if (!t.Actor.Physics.Phasing)
                                cost += cell.Tile.TileProperties.MovementCost;
                            return ActorMoved.Handle(new(t.Actor, oldPos, newPos));
                        }
                        else
                        {
                            var feature = featuresHere
                                .Single(x => x.Physics.BlocksMovement && !x.Physics.Phasing);
                            _ = ActorBumpedObstacle.Raise(new(t.Actor, feature));
                            // you can bump shrines and chests to interact with them
                            action = new InteractWithFeatureAction(feature);
                            cost = HandleAction(t, ref action);
                        }
                    }
                    else
                    {
                        var target = actorsHere
                            // Fake entities such as dummies may overlap the same tile as another actor.
                            .First(x => x.ActorProperties.Type != ActorName.None || x.Physics.BlocksMovement);
                        var relationship = _factionSystem.GetRelations(t.Actor, target).Left;
                        if (relationship.MayAttack())
                        {
                            action = new MeleeAttackOtherAction(target, t.Actor.ActorEquipment.Weapons.ToArray());
                            cost = HandleAction(t, ref action);
                            // Melee-fighting on difficult terrain is the same as moving through it
                            if (!t.Actor.Physics.Phasing)
                                cost += cell.Tile.TileProperties.MovementCost;
                        }
                        else if (relationship.IsFriendly() && target.Physics.Roots <= 0)
                        {
                            if (!t.Actor.Physics.Phasing)
                                cost += cell.Tile.TileProperties.MovementCost;
                            // you can swap position with allies in twice the amount of time it takes to move
                            cost *= 2;
                            t.Actor.Log?.Write($"$Action.YouSwapPlacesWith$ {target.Info.Name}.");
                            target.Log?.Write($"$Action.YouSwapPlacesWith$ {t.Actor.Info.Name}.");
                            return ActorMoved.Handle(new(t.Actor, oldPos, newPos))
                                   && ActorMoved.Handle(new(target, newPos, oldPos));
                        }
                    }
                }
                else
                {
                    _ = ActorBumpedObstacle.Raise(new(t.Actor, cell.Tile));
                    return false;
                }
            }
            else
            {
                // Bumped "nothingness"
                _ = ActorBumpedObstacle.Raise(new(t.Actor, null));
                return false;
            }
            return true;
        }
    }
}
