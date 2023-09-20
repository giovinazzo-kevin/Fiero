namespace Fiero.Business
{

    [TransientDependency]
    public partial class AiActionProvider : ActionProvider
    {
        protected Chance RepathChance { get; set; } = new(1, 1);

        private StateName _state = StateName.Wandering;

        public AiActionProvider(GameSystems systems)
            : base(systems)
        {
        }

        protected virtual StateName UpdateState(StateName state)
        {
            if (Panic)
            {
                return StateName.Retreating;
            }
            if (NearbyEnemies.Values.Count == 0)
            {
                return StateName.Wandering;
            }
            return StateName.Fighting;
        }


        public override bool RequestDelay => false;

        public override bool TryTarget(Actor a, TargetingShape shape, bool autotargetSuccesful)
        {
            return autotargetSuccesful && shape.GetPoints().Any(p => Systems.Dungeon.GetActorsAt(a.FloorId(), p).Any());
        }
        protected virtual IAction Retreat(Actor a)
        {
            if (MyPanicButtons.Values.Count > 0)
            {
                foreach (var item in MyPanicButtons.Values.Shuffle(Rng.Random))
                {
                    if (TryUseItem(a, item, out var action))
                    {
                        return action;
                    }
                }
            }
            if (GetClosestHostile(a) is { } hostile)
            {
                var dir = a.Position() - hostile.Position();
                if (!Systems.Dungeon.TryGetCellAt(a.FloorId(), a.Position() + dir, out var cell))
                {
                    return Fight(a);
                }
                if (!cell.IsWalkable(a))
                {
                    return Fight(a);
                }
                return new MoveRelativeAction(dir);
            }
            return new WaitAction();
        }

        protected virtual IAction Fight(Actor a)
        {
            IAction action;
            if (GetClosestHostile(a) is { } hostile)
            {
                TryPushObjective(a, hostile);
                if (a.IsInMeleeRange(hostile))
                {
                    return new MeleeAttackOtherAction(hostile, a.ActorEquipment.Weapons.ToArray());
                }

                if (MyWeapons.AlertingValues.FirstOrDefault() is { } betterWeapon)
                {
                    return new EquipItemAction(betterWeapon);
                }
                if (NearbyEnemies.Values.Count > 0 && MyHarmfulConsumables.Values.Count > 0 && Rng.Random.NChancesIn(2, 3))
                {
                    foreach (var item in MyHarmfulConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }
                if (NearbyAllies.Values.Count > 0 && MyHelpfulConsumables.Values.Count > 0 && Rng.Random.NChancesIn(1, 2))
                {
                    foreach (var item in MyHelpfulConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }
                if (MyUnidentifiedConsumables.Values.Count > 0 && Rng.Random.NChancesIn(1, 4))
                {
                    foreach (var item in MyUnidentifiedConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }

                if (TryFollowPath(a, out action))
                {
                    return action;
                }
            }
            return new WaitAction();
        }

        protected virtual void Repath(Actor a)
        {
            var floorId = a.FloorId();
            // Stick to the leader
            if (a.Party?.Leader is { } leader)
            {
                // Find a non-occupied tile that either you or the leader know
                if (Systems.Dungeon.TryGetClosestFreeTile(floorId, leader.Position(), out var closestToPlayer,
                    pred: c => c.IsWalkable(a) && (a.Fov.KnownTiles[floorId].Contains(c.Tile.Position())
                                                || leader.Fov.KnownTiles[floorId].Contains(c.Tile.Position()))))
                {
                    TryPushObjective(a, closestToPlayer.Tile);
                }
            }
            // Explore
            else if (TryGetUnexploredCandidate(a, out var tile))
            {
                TryPushObjective(a, tile);
            }
        }

        protected virtual IAction Wander(Actor a)
        {
            if (NearbyItems.AlertingValues.Count > 0)
            {
                var closestItem = NearbyItems.AlertingValues.First();
                if (a.Position() == closestItem.Position())
                {
                    return new PickUpItemAction(closestItem);
                }
                TryPushObjective(a, closestItem);
            }
            if (TryFollowPath(a, out var action))
            {
                return action;
            }
            if (a.Ai.Target == null && RepathChance.Check(Rng.Random))
            {
                Repath(a);
                if (TryFollowPath(a, out action))
                {
                    return action;
                }
            }
            return new MoveRandomlyAction();
        }

        public override IAction GetIntent(Actor a)
        {
            base.GetIntent(a);
            return (_state = UpdateState(_state)) switch
            {
                StateName.Retreating => Retreat(a),
                StateName.Fighting => Fight(a),
                StateName.Wandering => Wander(a),
                _ => throw new NotSupportedException(_state.ToString()),
            };
        }
    }
}


/*
 

            var floorId = a.FloorId();
            if (!Systems.Floor.TryGetFloor(floorId, out var floor))
                throw new ArgumentException(nameof(floorId));

            if (a.Ai.Target is null || !a.Ai.Target.IsAlive()) {
                a.Ai.Target = null; // invalidation
            }
            if (a.Ai.Target == null) {
                // Seek new target to attack
                if (!a.Fov.VisibleTiles.TryGetValue(floorId, out var fov)) {
                    return new MoveRandomlyAction();
                }
                var target = Systems.Faction.GetRelations(a)
                    .Where(r => r.Standing.IsHostile() && fov.Contains(r.Actor.Position()))
                    .Select(r => r.Actor)
                    .FirstOrDefault()
                    ?? fov.SelectMany(c => Systems.Floor.GetActorsAt(floorId, c))
                    .FirstOrDefault(b => Systems.Faction.GetRelations(a, b).Left.IsHostile());
                if (target != null) {
                    a.Ai.Target = target;
                }
            }
            if (a.Ai.Target != null) {
                if (a.Ai.Target.DistanceFrom(a) < 2) {
                    return new MeleeAttackOtherAction(a.Ai.Target, a.Equipment.Weapon);
                }
                if (a.CanSee(a.Ai.Target)) {
                    // If we can see the target and it has moved, recalculate the path
                    a.Ai.Path = floor.Pathfinder.Search(a.Position(), a.Ai.Target.Position(), default);
                    a.Ai.Path?.RemoveFirst();
                }
            }
            // Path to a random tile
            if (a.Ai.Path == null && Rng.Random.OneChanceIn(5)) {
                var randomTile = floor.Cells.Values.Shuffle(Rng.Random).Where(c => c.Tile.TileProperties.Name == TileName.Room
                    && c.IsWalkable(null)).First();
                a.Ai.Path = floor.Pathfinder.Search(a.Position(), randomTile.Tile.Position(), default);
            }
            // If following a path, do so until the end or an obstacle is reached
            else if (a.Ai.Path != null) {
                if (a.Ai.Path.First != null) {
                    var pos = a.Ai.Path.First.Value.Tile.Position();
                    var dir = new Coord(pos.X - a.Position().X, pos.Y - a.Position().Y);
                    var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                    a.Ai.Path.RemoveFirst();
                    if (diff > 0 && diff <= 2) {
                        // one tile ahead
                        return new MoveRelativeAction(dir);
                    }
                }
                else {
                    a.Ai.Path = null;
                    return GetIntent(a);
                }
            }
            return new MoveRandomlyAction();
 
 */