using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fiero.Business
{

    public class DungeonGenerator
    {
        protected readonly HashSet<DungeonGenerationNode> Nodes;
        protected DungeonGenerationNode StartNode { get; private set; }

        public DungeonGenerationSettings Settings { get; set; }

        public DungeonGenerator(DungeonGenerationSettings settings)
        {
            Nodes = new HashSet<DungeonGenerationNode>();
            Settings = settings;
        }


        protected virtual void Start()
        {
            Nodes.Clear();
            Nodes.Add(StartNode = new DungeonGenerationNode(DungeonNodeType.Start));
            DecorateRoom(StartNode);
        }
        
        protected virtual DungeonGenerationNode AddRoom()
        {
            // Pick the node that has the lowest min distance from the start node across any possible path and the most free connectors
            // Choose a random free connector (NSEW) and add a new normal room there (specialization is done later on)
            var takenPositions = Nodes.Select(n => n.Position).ToHashSet();
            var connectors = Nodes.Where(n => n.NumFreeConnectors > 0);
            var connect = connectors
                .Where(n => n.Type == DungeonNodeType.Normal || n.Type == DungeonNodeType.Enemies || n.Type == DungeonNodeType.Start)
                .Where(n => ManhattanDist(StartNode.Position, n.Position) < Math.Sqrt(Settings.NumRooms) - 1)
                .OrderBy(n => ManhattanDist(StartNode.Position, n.Position) + (4 - n.NumFreeConnectors) * 10 * Settings.PathRandomness)
                .ThenBy(n => n.NumFreeConnectors)
                .Select(n => {
                    var choices = new List<Func<DungeonGenerationNode, Coord>>();
                    if (n.North is null && !takenPositions.Contains(new(n.Position.X, n.Position.Y - 1))) {
                        choices.Add(n.GetConnectorNorth());
                    }
                    if (n.South is null && !takenPositions.Contains(new(n.Position.X, n.Position.Y + 1))) {
                        choices.Add(n.GetConnectorSouth());
                    }
                    if (n.East is null && !takenPositions.Contains(new(n.Position.X + 1, n.Position.Y))) {
                        choices.Add(n.GetConnectorEast());
                    }
                    if (n.West is null && !takenPositions.Contains(new(n.Position.X - 1, n.Position.Y))) {
                        choices.Add(n.GetConnectorWest());
                    }
                    if(choices.Count > 0) {
                        return choices[Rng.Random.Next(choices.Count)];
                    }
                    return null;
                })
                .Where(c => c != null)
                .First();
            var room = new DungeonGenerationNode(DungeonNodeType.Normal);
            room.Position = connect(room);
            // Track the room for faster retrieval
            Nodes.Add(room);
            return room;
        }

        static int ManhattanDist(Coord a, Coord b)
            => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        protected virtual void SpecializeRoom(DungeonNodeType type, int? targetDistance)
        {
            // Pick the normal node the distance from the start of which is closest to the target distance, and that has the most free connectors
            var node = Nodes.Where(n => n.Type == DungeonNodeType.Normal && n.CanPlaceNeighbor(type))
                .OrderByDescending(n => n.NumFreeConnectors)
                .ThenBy(n => targetDistance == null ? 1 : Math.Abs(StartNode.GetMinimumPathLength(n) - targetDistance.Value))
                .FirstOrDefault();
            if (node == null) {
                AddRoom();
                SpecializeRoom(type, null);
                return;
            }
            node.Type = type;
            if(node.Type == DungeonNodeType.Secret) {
                foreach (var con in node.GetUsedConnectors()) {
                    con(null);
                }
            }
            if(node.NumUsedConnectors > 1 && 
                (node.Type == DungeonNodeType.Boss || node.Type == DungeonNodeType.Item || node.Type == DungeonNodeType.Shop)) {
                var connectors = node.GetUsedConnectors().Take(node.NumUsedConnectors - 1);
                foreach (var con in connectors) {
                    con(null);
                }
            }
            DecorateRoom(node);
        }

        protected void DecorateRoom(DungeonGenerationNode node)
        {
            node.Objects.Clear();
            switch(node.Type) {
                case DungeonNodeType.Start: DecorateStart(); break;
                case DungeonNodeType.Item: DecorateItem(); break;
                case DungeonNodeType.Shop: DecorateShop(); break;
                case DungeonNodeType.Boss: DecorateBoss(); break;
                case DungeonNodeType.Enemies: DecorateEnemies(); break;
                case DungeonNodeType.Normal: DecorateNormal(); break;
                case DungeonNodeType.Secret: DecorateSecret(); break;
            };

            void DecorateStart()
            {
                node.Objects.Add(DungeonObjectName.Upstairs);
            }

            void DecorateNormal()
            {
                if (Rng.Random.NextDouble() < 0.30) {
                    node.Objects.Add(Rng.Random.Next(0, 2) switch {
                        0 => DungeonObjectName.Shrine,
                        _ => DungeonObjectName.Chest
                    });
                }
            }

            void DecorateShop()
            {
                var itemsSold = 0;
                for (var i = 0; i < 2; i++) {
                    if (Rng.Random.NextDouble() < 0.25) {
                        node.Objects.Add(DungeonObjectName.ItemForSale);
                        itemsSold++;
                    }
                }
                var consumablesToSell = 7 - itemsSold - Rng.Random.Next(0, 3);
                for (var i = 0; i < consumablesToSell; i++) {
                    node.Objects.Add(DungeonObjectName.ConsumableForSale);
                }
            }

            void DecorateEnemies()
            {
                if (Rng.Random.NextDouble() < 0.10) {
                    node.Objects.Add(Rng.Random.Next(0, 2) switch {
                        0 => DungeonObjectName.Trap,
                        _ => DungeonObjectName.Chest
                    });
                }
                node.Objects.Add(DungeonObjectName.Enemy);
                for (var i = 0; i < 9; i++) {
                    if (Rng.Random.NextDouble() < 0.11) {
                        node.Objects.Add(DungeonObjectName.Enemy);
                    }
                }
            }

            void DecorateItem()
            {
                for (var i = 0; i < 10; i++) {
                    node.Objects.Add(DungeonObjectName.Item);
                }
                for (var i = 0; i < 4; i++) {
                    if (Rng.Random.NextDouble() < 0.10) {
                        node.Objects.Add(DungeonObjectName.Consumable);
                    }
                }
            }

            void DecorateSecret()
            {
                if(Rng.Random.NextDouble() < 0.5) {
                    node.Objects.Add(DungeonObjectName.Item);
                }
                else {
                    if (Rng.Random.NextDouble() < 0.5) {
                        node.Objects.Add(DungeonObjectName.Consumable);
                        node.Objects.Add(DungeonObjectName.Consumable);
                        node.Objects.Add(DungeonObjectName.Consumable);
                    }
                }
                for (var i = 0; i < 3; i++) {
                    if (Rng.Random.NextDouble() < 0.50) {
                        node.Objects.Add(DungeonObjectName.Consumable);
                    }
                }
            }

            void DecorateBoss()
            {
                node.Objects.Add(DungeonObjectName.Boss);
                for (var i = 0; i < 9; i++) {
                    if (Rng.Random.NextDouble() < 0.11) {
                        node.Objects.Add(DungeonObjectName.Enemy);
                    }
                }
                if (!Nodes.Any(n => n.Objects.Contains(DungeonObjectName.Downstairs))) {
                    node.Objects.Add(DungeonObjectName.Downstairs);
                }
            }
        }

        protected virtual void Interconnect()
        {
            // Pick a random pair of rooms that are geometrically adjacent but not connected
            var nodes = Nodes.Where(n => n.Type == DungeonNodeType.Normal || n.Type == DungeonNodeType.Enemies)
                .ToList();
            var pairs = nodes
                .Select(node => {
                    var candidates = new List<Func<DungeonGenerationNode, Coord>>();
                    if (node.North == null && nodes.FirstOrDefault(n => n.Position == new Coord(node.Position.X, node.Position.Y - 1)) is { } north) {
                        candidates.Add(north.GetConnectorSouth());
                    }
                    if (node.South == null && nodes.FirstOrDefault(n => n.Position == new Coord(node.Position.X, node.Position.Y + 1)) is { } south) {
                        candidates.Add(south.GetConnectorNorth());
                    }
                    if (node.East == null && nodes.FirstOrDefault(n => n.Position == new Coord(node.Position.X + 1, node.Position.Y)) is { } east) {
                        candidates.Add(east.GetConnectorWest());
                    }
                    if (node.West == null && nodes.FirstOrDefault(n => n.Position == new Coord(node.Position.X - 1, node.Position.Y)) is { } west) {
                        candidates.Add(west.GetConnectorEast());
                    }
                    return (Node: node, Candidates: candidates);
                })
                .Where(s => s.Candidates.Count > 0)
                .ToList();
            if (pairs.Count == 0)
                return;
            var (node, connectors) = pairs[Rng.Random.Next(pairs.Count)];
            var connect = connectors[Rng.Random.Next(connectors.Count)];
            connect(node);
        }

        protected virtual void ReconnectOrphans()
        {
            foreach (var orphan in Nodes.ToList().Where(n => n.Type != DungeonNodeType.Secret && n.GetMinimumPathLength(StartNode) == Int32.MaxValue)) {
                var neighbors = Nodes.Where(n => ManhattanDist(n.Position, orphan.Position) == 1)
                    .OrderBy(n => n.Type switch {
                        DungeonNodeType.Normal => 10,
                        DungeonNodeType.Shop => -5,
                        DungeonNodeType.Item => -7,
                        DungeonNodeType.Boss => -10,
                        DungeonNodeType.Secret => -100,
                        _ => 0
                    });
                if(neighbors.FirstOrDefault() is { } n) {
                    if (n.Position == new Coord(orphan.Position.X, orphan.Position.Y - 1))
                        n.GetConnectorSouth()(orphan);
                    if (n.Position == new Coord(orphan.Position.X, orphan.Position.Y + 1))
                        n.GetConnectorNorth()(orphan);
                    if (n.Position == new Coord(orphan.Position.X - 1, orphan.Position.Y))
                        n.GetConnectorEast()(orphan);
                    if (n.Position == new Coord(orphan.Position.X + 1, orphan.Position.Y))
                        n.GetConnectorWest()(orphan);
                }
            }
        }

        public Dungeon Generate()
        {
            Start();
            for (var i = 0; i < Settings.NumRooms; i++) {
                AddRoom();
            }
            var maxDistance = Nodes.Max(n => StartNode.GetMinimumPathLength(n));
            var numInterconnect = (int)(Nodes.Count * Settings.InterconnectWeight);
            for (var i = 0; i < numInterconnect; i++) {
                Interconnect();
            }
            var numCorridor = (int)(Nodes.Count * Settings.CorridorChance);
            for (var i = 0; i < numCorridor; i++) {
                SpecializeRoom(DungeonNodeType.Corridor, (int)(maxDistance * 0.10));
            }
            for (var i = 0; i < Settings.NumItemRooms; i++) {
                SpecializeRoom(DungeonNodeType.Item, (int)(maxDistance * 0.50));
            }
            for (var i = 0; i < Settings.NumShopRooms; i++) {
                SpecializeRoom(DungeonNodeType.Shop, (int)(maxDistance * 0.75));
            }
            for (var i = 0; i < Settings.NumBossRooms; i++) {
                SpecializeRoom(DungeonNodeType.Boss, (int)(maxDistance * 1.00));
            }
            for (var i = 0; i < Settings.NumSecretRooms; i++) {
                SpecializeRoom(DungeonNodeType.Secret, (int)(maxDistance * 0.50));
            }
            var numNormalRooms = Nodes.Count(n => n.Type == DungeonNodeType.Normal);
            var numEnemyRooms = (int)(numNormalRooms * 0.50);
            for (var i = 0; i < numEnemyRooms; i++) {
                SpecializeRoom(DungeonNodeType.Enemies, Rng.Random.Next(maxDistance));
            }
            ReconnectOrphans();
            foreach (var normal in Nodes.Where(n => n.Type == DungeonNodeType.Normal)) {
                DecorateRoom(normal);
            }

            Console.WriteLine("Item Room: " + Nodes.First(n => n.Type == DungeonNodeType.Item).Position.ToString());
            Console.WriteLine("Shop Room: " + Nodes.First(n => n.Type == DungeonNodeType.Shop).Position.ToString());
            Console.WriteLine("Boss Room: " + Nodes.First(n => n.Type == DungeonNodeType.Boss).Position.ToString());
            Console.WriteLine("Secret Room: " + Nodes.First(n => n.Type == DungeonNodeType.Secret).Position.ToString());
            Console.WriteLine("Normal Rooms: " + Nodes.Count(n => n.Type == DungeonNodeType.Normal));
            Console.WriteLine("Enemies Rooms: " + Nodes.Count(n => n.Type == DungeonNodeType.Enemies));
            return new Dungeon(Nodes.ToImmutableHashSet(), Settings);
        }
    }
}
