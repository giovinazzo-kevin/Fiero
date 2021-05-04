using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class DungeonGenerationNode
    {
        public DungeonNodeType Type { get; set; }

        public Coord Position { get; set; }
        public DungeonGenerationNode North { get; set; }
        public DungeonGenerationNode South { get; set; }
        public DungeonGenerationNode East { get; set; }
        public DungeonGenerationNode West { get; set; }

        public List<DungeonObjectName> Objects { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is DungeonGenerationNode other) {
                return other.Type == Type && other.Position == Position;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Type);
            hash.Add(Position);
            return hash.ToHashCode();
        }

        public bool CanPlaceNeighbor(DungeonNodeType type)
        {
            return type switch {
                DungeonNodeType.Item => Allow(DungeonNodeType.Normal),
                DungeonNodeType.Shop => Allow(DungeonNodeType.Normal),
                DungeonNodeType.Boss => NumFreeConnectors == 3 && Allow(DungeonNodeType.Boss) || Allow(DungeonNodeType.Normal),
                DungeonNodeType.Secret => Allow(DungeonNodeType.Normal),
                _ => true
            };

            bool Allow(params DungeonNodeType[] t) =>
                (North == null || t.Contains(North.Type)) &&
                (South == null || t.Contains(South.Type)) &&
                (East == null || t.Contains(East.Type)) &&
                (West == null || t.Contains(West.Type));
        }

        public int NumFreeConnectors =>
            (North is null ? 1 : 0) +
            (South is null ? 1 : 0) +
            (East  is null ? 1 : 0) +
            (West  is null ? 1 : 0) ;
        public int NumUsedConnectors => 4 - NumFreeConnectors;


        public Func<DungeonGenerationNode, Coord> GetConnectorNorth()
            => n => { if (North != null) North.South = null; North = n; if(n != null) n.South = this; return new(Position.X, Position.Y - 1); };
        public Func<DungeonGenerationNode, Coord> GetConnectorSouth()
            => n => { if (South != null) South.North = null; South = n; if (n != null) n.North = this; return new(Position.X, Position.Y + 1); };
        public Func<DungeonGenerationNode, Coord> GetConnectorEast()
            => n => { if (East != null) East.West = null; East = n; if (n != null) n.West = this; return new(Position.X + 1, Position.Y); };
        public Func<DungeonGenerationNode, Coord> GetConnectorWest()
            => n => { if (West != null) West.East = null; West = n; if (n != null) n.East = this; return new(Position.X - 1, Position.Y); };

        public IEnumerable<Func<DungeonGenerationNode, Coord>> GetFreeConnectors()
        {
            var choices = new List<Func<DungeonGenerationNode, Coord>>();
            if (North is null) {
                choices.Add(GetConnectorNorth());
            }
            if (South is null) {
                choices.Add(GetConnectorSouth());
            }
            if (East is null) {
                choices.Add(GetConnectorEast());
            }
            if (West is null) {
                choices.Add(GetConnectorWest());
            }
            return choices;
        }

        public IEnumerable<Func<DungeonGenerationNode, Coord>> GetUsedConnectors()
        {
            var choices = new List<Func<DungeonGenerationNode, Coord>>();
            if (North != null) {
                choices.Add(GetConnectorNorth());
            }
            if (South != null) {
                choices.Add(GetConnectorSouth());
            }
            if (East != null) {
                choices.Add(GetConnectorEast());
            }
            if (West != null) {
                choices.Add(GetConnectorWest());
            }
            return choices;
        }

        public DungeonGenerationNode(DungeonNodeType type)
        {
            Type = type;
            Objects = new List<DungeonObjectName>();
        }

        public int GetMinimumPathLength(DungeonGenerationNode node, int len = 0, ImmutableHashSet<DungeonGenerationNode> traversed = null)
        {
            traversed ??= ImmutableHashSet.Create<DungeonGenerationNode>();
            if (node == this)
                return len;
            var costs = new List<int>();
            if (node.North != null && !traversed.Contains(node.North)) {
                costs.Add(GetMinimumPathLength(node.North, len + 1, traversed.Add(node)));
            }
            if (node.South != null && !traversed.Contains(node.South)) {
                costs.Add(GetMinimumPathLength(node.South, len + 1, traversed.Add(node)));
            }
            if (node.East != null && !traversed.Contains(node.East)) {
                costs.Add(GetMinimumPathLength(node.East, len + 1, traversed.Add(node)));
            }
            if (node.West != null && !traversed.Contains(node.West)) {
                costs.Add(GetMinimumPathLength(node.West, len + 1, traversed.Add(node)));
            }
            if (costs.Count == 0) {
                return Int32.MaxValue;
            }
            return costs.Min();
        }
    }
}
