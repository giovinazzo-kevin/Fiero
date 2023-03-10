using Ergo.Lang.Extensions;
using Fiero.Core;
using Fiero.Core.Extensions;
using Fiero.Core.Structures;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class RoomSector : IFloorGenerationPrefab
    {
        public readonly bool[] Cells;
        public readonly IntRect Sector;
        public readonly Room[] Rooms;
        public readonly Corridor[] Corridors;

        public RoomSector(IntRect sector, bool[] cells, Func<Room> makeRoom, int nBestCorridors)
        {
            if (cells.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(cells));
            Sector = sector;
            Cells = cells;

            var subdivisions = Sector.Subdivide(new(4, 4));
            var rects = subdivisions
                .Select((r, i) => (Index: i, Rect: r))
                .Where(r => cells[r.Index])
                .ToDictionary(r => r.Index, r => r.Rect);
            var groups = new List<List<int>>();
            foreach (var key in rects.Keys)
            {
                var added = false;
                foreach (var group in groups)
                {
                    if (CardinallyAdjacent(group, key))
                    {
                        group.Add(key);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    groups.Add(new() { key });
                }
            }

            // merge adjacent groups (technically this shouldn't be necessary but... TODO: verify)
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (groups[i].Any(x => CardinallyAdjacent(groups[j], x)))
                    {
                        groups[i].AddRange(groups[j]);
                        groups.RemoveAt(j);

                        j--; i--;
                    }
                }
            }

            Rooms = groups
                .Select(s =>
                {
                    var room = makeRoom();
                    foreach (var i in s)
                    {
                        room.AddRect(i, rects[i]);
                    }
                    return room;
                })
                .ToArray();
            Corridors = GenerateIntraSectorCorridors(Rooms, nBestCorridors).ToArray();
            Console.WriteLine($"Rooms: {Rooms.Length}; Corridors: {Corridors.Length}");
        }

        public static IEnumerable<Corridor> GenerateIntraSectorCorridors(IList<Room> rooms, int nBest = 1)
        {
            var connectedRooms = new HashSet<UnorderedPair<Room>>();
            var roomPairs = rooms.SelectMany(r => rooms.Where(s => s != r).Select(s => new UnorderedPair<Room>(r, s)))
                .OrderBy(p => p.Left.Position.DistSq(p.Right.Position))
                .ToList();
            foreach (var rp in roomPairs)
            {
                if (connectedRooms.Contains(rp)
                    || connectedRooms.Any(c => c.Left == rp.Left && connectedRooms.Contains(new(c.Right, rp.Right)))
                    || connectedRooms.Any(c => c.Right == rp.Left && connectedRooms.Contains(new(c.Left, rp.Right)))
                    || connectedRooms.Any(c => c.Left == rp.Right && connectedRooms.Contains(new(c.Right, rp.Left)))
                    || connectedRooms.Any(c => c.Right == rp.Right && connectedRooms.Contains(new(c.Left, rp.Left))))
                {
                    continue;
                }
                var connectorPairs = rp.Left.GetConnectors()
                    .SelectMany(c => rp.Right.GetConnectors()
                        .Select(d => new UnorderedPair<RoomConnector>(c, d)))
                    .Where(p => new Line(p.Left.Edge.Left, p.Left.Edge.Right)
                        .IsParallel(new Line(p.Right.Edge.Left, p.Right.Edge.Right)))
                    .ToList();
                foreach (var bestPair in connectorPairs
                    .OrderBy(p => p.Right.Center.DistSq(p.Left.Center))
                    .Take(nBest))
                {
                    var corridor = new Corridor(bestPair.Left.Edge, bestPair.Right.Edge);
                    if (!rooms.Any(r => r.GetRects().Any(r => corridor.Points.Skip(1).SkipLast(1).Any(p => r.Contains(p.X, p.Y)))))
                    {
                        connectedRooms.Add(rp);
                        yield return corridor;
                    }
                }
            }
        }

        public static IEnumerable<Corridor> GenerateInterSectorCorridors(IList<RoomSector> sectors, int nBest = 1)
        {
            var indexed = sectors.Select((s, i) => (Sector: s, Index: i))
                .ToList();
            var side = (int)Math.Sqrt(indexed.Count);
            var connected = new List<UnorderedPair<Room>>();
            foreach (var s in indexed)
            {
                var c = new Coord(s.Index % side, s.Index / side);
                foreach (var S in indexed.Where(x => new Coord(x.Index % side, x.Index / side).DistSq(c) == 1))
                {
                    var availableConnectors = S.Sector.Rooms.SelectMany(r => r.GetConnectors());
                    var myConnectors = s.Sector.Rooms
                        .SelectMany(r => r.GetConnectors());
                    var pairs = myConnectors.SelectMany(c => availableConnectors.Select(d => new UnorderedPair<RoomConnector>(c, d)))
                        .ToList();
                    foreach (var bestPair in pairs
                        .OrderBy(p => p.Right.Center.Dist(p.Left.Center))
                        .Take(nBest))
                    {
                        var conn = new UnorderedPair<Room>(bestPair.Left.Owner, bestPair.Right.Owner);
                        if (connected.Count(x => x == conn) >= nBest)
                        {
                            continue;
                        }
                        connected.Add(conn);
                        yield return new(bestPair.Left.Edge, bestPair.Right.Edge);
                    }
                }
            }
        }

        record class Node(Coord G, RoomSector Item, Node Right = null, Node Bottom = null)
        {
            public static int[] TopEdge = new[] { 0, 1, 2, 3 };
            public static int[] LeftEdge = new[] { 0, 4, 8, 12 };
            public static int[] RightEdge = new[] { 3, 7, 11, 15 };
            public static int[] BottomEdge = new[] { 12, 13, 14, 15 };
        };
        public static IEnumerable<RoomSector> CreateTiling(Coord sectorScale, Coord gridSize, Func<Room> makeRoom, int nBestCorridors = 1)
        {
            // Create a grid of sectors such that:
            // - There are no diagonal gaps between rooms of adjacent sectors
            var nodes = new Dictionary<Coord, Node>();
            for (int y = gridSize.Y; y > 0; y--)
            {
                for (int x = gridSize.X; x > 0; x--)
                {
                    var g = new Coord(x, y);
                    var sector = new IntRect((g - Coord.PositiveOne) * sectorScale, sectorScale);
                    nodes.TryGetValue(g + Coord.PositiveX, out var right);
                    nodes.TryGetValue(g + Coord.PositiveY, out var bottom);
                    var node = new Node(g, null, right, bottom);
                    nodes[g] = node with { Item = Create(sector, makeRoom, i => Constrain(node, i), nBestCorridors) };
                }
            }
            return nodes.Values.Select(v => v.Item);

            bool Constrain(Node n, int i)
            {
                var p = Coord.FromIndex(i, 4);
                if (n.Right is { Item: { Cells: var R }, Bottom: var bot })
                {
                    var q = p - Coord.PositiveX * 2;
                    if (q.X == 1 && DiagonallyAdjacent(Node.LeftEdge.Where(i => R[i]), q.ToIndex(4)))
                        return false;
                    if (bot?.Right is { Item: { Cells: var BR } })
                    {
                        q -= Coord.PositiveY * 2;
                        if (q.Y == 1 && DiagonallyAdjacent(BR[0] ? new int[] { 0 } : Enumerable.Empty<int>(), q.ToIndex(4)))
                            return false;
                    }
                }
                if (n.Bottom is { Item: { Cells: var B } })
                {
                    var q = p - Coord.PositiveY * 2;
                    if (q.Y == 1 && DiagonallyAdjacent(Node.TopEdge.Where(i => B[i]), q.ToIndex(4)))
                        return false;
                }

                return true;
            }
        }

        public static RoomSector Create(IntRect sector, Func<Room> makeRoom, Func<int, bool> constrain = null, int nBestCorridors = 1)
        {
            constrain ??= _ => true;
            var mat = new bool[16];
            var candidates = new HashSet<int>();
            var indices = Enumerable.Range(0, 16).Shuffle(Rng.Random).ToArray();
            // Iteratively fill up to 8 squares in the 4x4 matrix according to these rules:
            // - You can choose a cell that has "tetris adjacency" to nearby cells
            var squares = Rng.Random.Between(3, 8);
            for (int i = 0; i < squares; i++)
            {
                foreach (var index in indices
                    .Where(i => !candidates.Contains(i))
                    .Where(constrain)
                    .Where(i => TetrisAdjacent(candidates, i))
                )
                {
                    candidates.Add(index);
                    mat[index] = true;
                    Print();
                    break;
                }

            }
            return new(sector, mat, makeRoom, nBestCorridors);

            void Print()
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Console.Write(mat[j * 4 + i] ? '#' : '.');
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }

        static Coord ToCoord(int a) => new(a / 4, a % 4);

        public static bool CardinallyAdjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            return candidates.Select(c => ToCoord(c))
                .Any(q => p.DistSq(q) < 2);
        }

        public static bool DiagonallyAdjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            return candidates.Select(c => ToCoord(c))
                .Any(q => p.DistSq(q) == 2);
        }

        public static bool Adjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            return candidates.Select(c => ToCoord(c))
                .Any(q => p.DistSq(q) <= 2);
        }

        // Checks whether this cell is cardinally adjacent to another cell
        // and NOT diagonally adjacent to any DISCONTIGUOUS cell
        // and doesn't have more than 3 other neighbors
        // and the contiguous block is shorter than N cells
        public static bool TetrisAdjacent(IEnumerable<int> candidates, int a, int maxLength = 4)
        {
            var map = candidates
                .Select(c => (Index: c, Coord: ToCoord(c)))
                .ToArray();

            var closedSet = new HashSet<int>();
            var openSet = new Queue<int>();
            openSet.Enqueue(a);

            while (openSet.TryDequeue(out var b))
            {
                var p = ToCoord(b);
                var neighbors = map
                    .Where(x => x.Coord.DistSq(p) < 2)
                    .Select(x => x.Index);
                foreach (var n in neighbors.Where(x => !closedSet.Contains(x)))
                {
                    openSet.Enqueue(n);
                }
                closedSet.Add(b);
            }

            var discontiguous = candidates.Where(c => !closedSet.Contains(c));

            var ret = !DiagonallyAdjacent(discontiguous, a)
                && closedSet.Count > 0
                && (maxLength <= 0 || closedSet.Count <= maxLength);
            if (ret)
            {
                Console.WriteLine(closedSet.Join(","));
            }
            return ret;
        }

        public void Draw(FloorGenerationContext ctx)
        {
            foreach (var room in Rooms)
            {
                ctx.Draw(room);
            }
            foreach (var corridor in Corridors)
            {
                ctx.Draw(corridor);
            }
        }
    }
}
