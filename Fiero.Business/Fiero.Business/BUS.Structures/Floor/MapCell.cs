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
            && !Features.Any(f => f.Physics.BlocksMovement);
        public IEnumerable<DrawableEntity> GetDrawables(bool seen = true)
        {
            // Only show the tile if there's nothing visible on it
            if(    !Items.Any(x => !x.Render.Hidden) 
                && !Features.Any(x => !x.Render.Hidden) 
                && (!seen || !Actors.Any(x => !x.Render.Hidden)))
                yield return Tile;
            foreach (var x in Features.Where(x => !x.Render.Hidden)) yield return x;
            foreach (var x in Items.Where(x => !x.Render.Hidden)) yield return x;
            if (!seen)
                yield break;
            foreach (var x in Actors.Where(x => !x.Render.Hidden)) yield return x;
        }

        public override string ToString()
        {
            return $"{String.Join(", ", GetDrawables())}.";
        }
    }
}
