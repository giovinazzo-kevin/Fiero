using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class MapCell : IPathNode<object>
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

        public bool IsWalkable(object inContext) => ((IPathNode<object>)Tile).IsWalkable(inContext)
            && !Features.Any(f => f.Physics.BlocksPathing);
        public IEnumerable<PhysicalEntity> GetDrawables(bool seen = true)
        {
            yield return Tile;
            foreach (var x in Features.Where(x => !x.Render.Hidden)) yield return x;
            foreach (var x in Items.Where(x => !x.Render.Hidden)) yield return x;
            if (!seen)
                yield break;
            foreach (var x in Actors.Where(x => !x.Render.Hidden)) yield return x;
        }

        public override string ToString() => ToString(true);
        public string ToString(bool seen)
        {
            return $"{String.Join(", ", GetDrawables(seen))}.";
        }
    }
}
