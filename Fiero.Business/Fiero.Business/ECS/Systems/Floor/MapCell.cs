using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            && !Features.Any(f => f.FeatureProperties.BlocksMovement);
        public IEnumerable<Drawable> GetDrawables(bool seen = true)
        {
            if(!Items.Any() && !Features.Any() && (!seen || !Actors.Any()))
                yield return Tile;
            foreach (var x in Features) yield return x;
            foreach (var x in Items) yield return x;
            if (!seen)
                yield break;
            foreach (var x in Actors) yield return x;
        }

        public override string ToString()
        {
            return $"{String.Join(", ", GetDrawables())}.";
        }
    }
}
