namespace Fiero.Business
{
    [TransientDependency]
    public class IdleActionProvider : ActionProvider
    {
        public IdleActionProvider(MetaSystem sys) : base(sys)
        {
        }

        public override bool RequestDelay => false;

        public override bool TryTarget(Actor a, TargetingShape shape, bool autotargetSuccesful) => false;
        public override IAction GetIntent(Actor actor) => new WaitAction();

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
                var target = Systems.Get<FactionSystem>().GetRelations(a)
                    .Where(r => r.Standing.IsHostile() && fov.Contains(r.Actor.Position()))
                    .Select(r => r.Actor)
                    .FirstOrDefault()
                    ?? fov.SelectMany(c => Systems.Floor.GetActorsAt(floorId, c))
                    .FirstOrDefault(b => Systems.Get<FactionSystem>().GetRelations(a, b).Left.IsHostile());
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