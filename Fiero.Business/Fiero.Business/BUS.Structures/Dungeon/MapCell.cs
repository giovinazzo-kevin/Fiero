namespace Fiero.Business
{
    public class MapCell : IPathNode<PhysicalEntity>
    {
        public Tile Tile { get; set; }
        public readonly HashSet<Actor> Actors;
        public readonly HashSet<Item> Items;
        public readonly HashSet<Feature> Features;

        public MapCell(Tile tile)
        {
            Tile = tile;
            Actors = new();
            Items = new();
            Features = new();
        }

        public bool IsWalkable(PhysicalEntity e)
        {
            var ret = e.Physics.Phasing
                || ((IPathNode<PhysicalEntity>)Tile).IsWalkable(e)
                && !BlocksMovement();
            if (e.TryCast<Actor>(out var a))
            {
                ret &= !Features.Any(f => a.IsPlayer() switch
                    {
                        true => f.Physics.BlocksPlayerPathing,
                        false => f.Physics.BlocksNpcPathing
                    });
            }
            return ret;
        }
        public double GetCost(PhysicalEntity e)
        {
            var cost = 0d;
            if (!IsWalkable(e)) return 1000; // arbitrarily high cost
            if (e.Physics.Phasing) return 1;
            if (e.Physics.Flying && e.Physics.Roots == 0) return 1; // flying units ignore pathing costs unless rooted to the ground
            cost += Tile.GetCost(e);
            return cost;
        }
        public bool BlocksMovement(bool excludeFlat = false)
        {
            var anyFeatures = Features.Any(f => f.Physics.BlocksMovement);
            if (!excludeFlat)
                return anyFeatures || Tile.Physics.BlocksMovement;
            return anyFeatures || Tile.Physics.BlocksMovement && !Tile.Physics.IsFlat;
        }
        public bool BlocksLight() => Features.Any(f => f.Physics.BlocksLight) || Tile.Physics.BlocksLight;
        public IEnumerable<PhysicalEntity> GetDrawables(VisibilityName visibility = VisibilityName.Visible, bool seen = true)
        {
            yield return Tile;
            foreach (var x in Features.Where(x => visibility.HasFlag(x.Render.Visibility))) yield return x;
            foreach (var x in Items.Where(x => visibility.HasFlag(x.Render.Visibility))) yield return x;
            if (!seen)
                yield break;
            foreach (var x in Actors.Where(x => visibility.HasFlag(x.Render.Visibility))) yield return x;
        }

        public override string ToString() => ToString(VisibilityName.Visible, true);
        public string ToString(VisibilityName visibility, bool seen)
        {
            return $"{String.Join(", ", GetDrawables(visibility, seen))}.";
        }
    }
}
