﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        public bool IsWalkable(PhysicalEntity e) => e.Physics.Phasing || ((IPathNode<PhysicalEntity>)Tile).IsWalkable(e)
            && !BlocksMovement() && !Features.Any(f => e.TryCast<Actor>(out var a) && a.IsPlayer() switch
            {
                true => f.Physics.BlocksPlayerPathing,
                false => f.Physics.BlocksNpcPathing
            });
        public bool BlocksMovement() => Features.Any(f => f.Physics.BlocksMovement) || Tile.Physics.BlocksMovement;
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