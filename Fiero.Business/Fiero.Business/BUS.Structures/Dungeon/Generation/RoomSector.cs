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
        public readonly Coord GridPos;
        public readonly List<Room> Rooms;
        public readonly List<Corridor> Corridors;

        public RoomSector(IntRect sector, Coord gridCoord, bool[] cells, Func<Room> makeRoom)
        {
            if (cells.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(cells));
            Sector = sector;
            Cells = cells;
            GridPos = gridCoord;
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
                .ToList();
            Corridors = GenerateIntraSectorCorridors(this).ToList();
            Console.WriteLine($"Rooms: {Rooms.Count}; Corridors: {Corridors.Count}");
        }

        public void MarkSecretCorridors(int roll)
        {
            var secrets = Enumerable.Range(0, Corridors.Count)
                .Shuffle(Rng.Random)
                .Take(roll)
                .ToArray();
            foreach (var i in secrets)
            {
                Corridors[i] = new SecretCorridor(Corridors[i].Start, Corridors[i].End);
            }
        }

        public static IEnumerable<Corridor> GenerateIntraSectorCorridors(RoomSector sector)
        {
            var connectedRooms = new HashSet<Room>();
            var roomPairs = sector.Rooms.Pairs()
                .OrderBy(p => p.Left.Position.DistSq(p.Right.Position));
            foreach (var rp in roomPairs)
            {
                if (connectedRooms.Contains(rp.Left) && connectedRooms.Contains(rp.Right))
                {
                    continue;
                }
                var connectorPairs = rp.Left.GetConnectors()
                    .Pairs(rp.Right.GetConnectors())
                    .Select(p => (Connector: p, Length: CorridorLength(p.Left, p.Right)))
                    .Where(p => p.Length > 0)
                    .OrderBy(p => p.Length)
                    .Select(p => p.Connector)
                    ;
                var workingSet = new HashSet<Corridor>();
                foreach (var item in Inner().Take(1))
                    yield return item;
                IEnumerable<Corridor> Inner()
                {
                    foreach (var bestPair in connectorPairs)
                    {
                        var corridor = new Corridor(bestPair.Left, bestPair.Right);
                        if (!IsCorridorOverlapping(new[] { sector }, corridor, workingSet))
                        {
                            connectedRooms.Add(rp.Left);
                            connectedRooms.Add(rp.Right);
                            workingSet.Add(corridor);
                            yield return corridor;
                        }
                    }
                    yield break;
                }
            }
        }

        public static IEnumerable<Corridor> GenerateInterSectorCorridors(IList<RoomSector> sectors, Dice nBest)
        {
            var connectedSectors = new HashSet<UnorderedPair<RoomSector>>();
            var sectorPairs = sectors.Pairs()
                .Where(s => s.Left.GridPos.CardinallyAdjacent(s.Right.GridPos))
                .OrderBy(p => p.Left.Sector.Position().DistSq(p.Right.Sector.Position()))
                .ToList()
                ;
            foreach (var sp in sectorPairs)
            {
                if (connectedSectors.Any(sp2 => sp2.GetHashCode() == sp.GetHashCode()))
                {
                    continue;
                }
                var connectorPairs = sp.Left.Rooms.SelectMany(r => r.GetConnectors())
                    .Pairs(sp.Right.Rooms.SelectMany(r => r.GetConnectors()))
                    .Select(p => (Connector: p, Length: CorridorLength(p.Left, p.Right)))
                    .Where(p => p.Length > 0)
                    .OrderBy(p => p.Length)
                    .Select(p => p.Connector)
                    .ToList();
                var workingSet = new HashSet<Corridor>(sectors
                    .SelectMany(x => x.Corridors));
                foreach (var item in Inner().Take(nBest.Roll().Sum()))
                    yield return item;
                IEnumerable<Corridor> Inner()
                {
                    foreach (var bestPair in connectorPairs)
                    {
                        var corridor = new Corridor(bestPair.Left, bestPair.Right);
                        if (!IsCorridorOverlapping(sectors, corridor, workingSet))
                        {
                            connectedSectors.Add(sp);
                            workingSet.Add(corridor);
                            yield return corridor;
                        }
                    }
                    yield break;
                }
            }
        }

        private static int CorridorLength(RoomConnector a, RoomConnector b) => new Corridor(a, b).Length;

        private static bool IsCorridorOverlapping(IEnumerable<RoomSector> sectors, Corridor corridor, HashSet<Corridor> workingSet)
        {
            var allRects = sectors.SelectMany(s => s.Rooms.SelectMany(r => r.GetRects()))
                .ToHashSet();
            foreach (var p in corridor.Points)
            {
                // Discard corridors that overlap any room, including the ones they are connected to
                if (allRects.Any(r => r.Contains(p.X, p.Y)))
                    return true;
                // Discard corridors that cross each other without merging at either endpoint
                if (workingSet.Any(x => new[] { corridor.Start, corridor.End, x.Start, x.End }.Distinct().Count() == 4
                    && x.Points.Contains(p)))
                    return true;
            }
            return false;
        }

        record class Node(Coord G, RoomSector Item, Node Right = null, Node Bottom = null)
        {
            public static int[] LeftEdge = new[] { 0, 1, 2, 3 };
            public static int[] TopEdge = new[] { 0, 4, 8, 12 };
            public static int[] BottomEdge = new[] { 3, 7, 11, 15 };
            public static int[] RightEdge = new[] { 12, 13, 14, 15 };
        };
        public static IEnumerable<RoomSector> CreateTiling(Coord mapSize, Coord gridSize, Func<Room> makeRoom)
        {
            var sectorScale = (mapSize - Coord.PositiveOne) / gridSize;
            // Create a grid of sectors such that:
            // - There are no diagonal gaps between rooms of adjacent sectors
            var nodes = new Dictionary<Coord, Node>();
            for (int y = gridSize.Y; y > 0; y--)
            {
                for (int x = gridSize.X; x > 0; x--)
                {
                    var g = new Coord(x, y);
                    var _g = g - Coord.PositiveOne;

                    var sector = new IntRect(_g * sectorScale, sectorScale);
                    nodes.TryGetValue(g + Coord.PositiveX, out var right);
                    nodes.TryGetValue(g + Coord.PositiveY, out var bottom);
                    var node = new Node(g, null, right, bottom);
                    nodes[g] = node with { Item = Create(sector, _g, makeRoom, i => Constrain(node, i)) };
                }
            }
            return nodes.Values.Select(v => v.Item);

            bool Constrain(Node n, int i)
            {
                var k = true;
                if (n.Right is { Item: var right })
                {
                    var indices = Node.LeftEdge
                        .Select((x, i) => (x, i))
                        .Where(a => right.Cells[a.x]);
                    var invalid = Node.RightEdge.Except(indices
                        .Select(a => Node.RightEdge[a.i]));
                    k &= !invalid.Contains(i);
                }
                if (n.Bottom is { Item: var bottom })
                {
                    var indices = Node.TopEdge
                        .Select((x, i) => (x, i))
                        .Where(a => bottom.Cells[a.x]);
                    var invalid = Node.BottomEdge.Except(indices
                        .Select(a => Node.BottomEdge[a.i]));
                    k &= !invalid.Contains(i);
                }
                return k;
            }
        }

        public static RoomSector Create(IntRect sector, Coord gridPos, Func<Room> makeRoom, Func<int, bool> constrain)
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
                    break;
                }

            }
            return new(sector, gridPos, mat, makeRoom);
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
        public static bool TetrisAdjacent(IEnumerable<int> candidates, int a, int maxLength = 6)
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

        public void MarkActiveConnectors(IEnumerable<Corridor> interSectorCorridors)
        {
            foreach (var room in Rooms)
            {
                var connectors = room.GetConnectors();
                var protectedConnectors = Corridors.Concat(interSectorCorridors)
                    .SelectMany(c => new[] { c.Start, c.End })
                    .Where(connectors.Contains);
                foreach (var conn in connectors)
                    conn.IsUsed = protectedConnectors.Contains(conn);
            }
        }

        public static void MarkSharedConnectors(IEnumerable<RoomSector> sectors)
        {
            var pairs = sectors.SelectMany(s => s.Rooms)
                .SelectMany(r => r.GetConnectors())
                .Pairs();
            foreach (var pair in pairs)
            {
                var a = Shapes.Line(pair.Left.Edge.Left, pair.Left.Edge.Right);
                var b = Shapes.Line(pair.Right.Edge.Left, pair.Right.Edge.Right);
                var ca = a.Count();
                var cb = b.Count();
                var ci = a.Intersect(b).Count();
                if (ci == ca || ci == cb)
                {
                    pair.Left.IsHidden = true;
                    pair.Right.IsHidden = true;
                }
            }
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
