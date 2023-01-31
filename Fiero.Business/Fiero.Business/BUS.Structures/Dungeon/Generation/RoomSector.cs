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
            foreach (var key in rects.Keys) {
                var added = false;
                foreach (var group in groups) {
                    if(CardinallyAdjacent(group, key)) {
                        group.Add(key);
                        added = true;
                        break;
                    }
                }
                if(!added) {
                    groups.Add(new() { key });
                }
            }

            Rooms = groups
                .Select(s => {
                    var room = makeRoom();
                    foreach (var i in s) {
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
            foreach (var rp in roomPairs) {
                if(connectedRooms.Contains(rp)
                    || connectedRooms.Any(c => c.Left == rp.Left && connectedRooms.Contains(new(c.Right, rp.Right)))
                    || connectedRooms.Any(c => c.Right == rp.Left && connectedRooms.Contains(new(c.Left, rp.Right)))
                    || connectedRooms.Any(c => c.Left == rp.Right && connectedRooms.Contains(new(c.Right, rp.Left)))
                    || connectedRooms.Any(c => c.Right == rp.Right && connectedRooms.Contains(new(c.Left, rp.Left)))) {
                    continue;
                }
                var connectorPairs = rp.Left.GetConnectors()
                    .SelectMany(c => rp.Right.GetConnectors()
                        .Select(d => new UnorderedPair<RoomConnector>(c, d)))
                    .Where(p => new Line(p.Left.Edge.Left, p.Left.Edge.Right).IsParallel(new Line(p.Right.Edge.Left, p.Right.Edge.Right)))
                    .ToList();
                foreach(var bestPair in connectorPairs
                    .OrderBy(p => p.Right.Center.DistSq(p.Left.Center))
                    .Take(nBest)) {
                    var corridor = new Corridor(bestPair.Left.Edge, bestPair.Right.Edge);
                    if (!rooms.Any(r => r.GetRects().Any(r => corridor.Points.Skip(1).SkipLast(1).Any(p => r.Contains(p.X, p.Y))))) {
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
            foreach (var s in indexed) {
                var c = new Coord(s.Index % side, s.Index / side);
                foreach (var S in indexed.Where(x => new Coord(x.Index % side, x.Index / side).DistSq(c) == 1)) {
                    var availableConnectors = S.Sector.Rooms.SelectMany(r => r.GetConnectors());
                    var myConnectors = s.Sector.Rooms
                        .SelectMany(r => r.GetConnectors());
                    var pairs = myConnectors.SelectMany(c => availableConnectors.Select(d => new UnorderedPair<RoomConnector>(c, d)))
                        .ToList();
                    foreach(var bestPair in pairs
                        .OrderBy(p => p.Right.Center.Dist(p.Left.Center))
                        .Take(nBest)) {
                        var conn = new UnorderedPair<Room>(bestPair.Left.Owner, bestPair.Right.Owner);
                        if (connected.Count(x => x == conn) >= nBest) {
                            continue;
                        }
                        connected.Add(conn);
                        yield return new(bestPair.Left.Edge, bestPair.Right.Edge);
                    }
                }
            }
        }

        public static RoomSector Create(IntRect sector, Func<Room> makeRoom, int nBestCorridors = 1)
        {
            var mat = new bool[16];
            var candidates = new HashSet<int>();
            var indices = Enumerable.Range(0, 16).Shuffle(Rng.Random).ToArray();
            // Iteratively fill 3-5 squares in the 4x4 matrix according to these rules:
            // - You can choose a cell that has no diagonal neighbors around itself
            var squares = Rng.Random.Between(3, 5);
            for (int i = 0; i < squares; i++) {
                var index = indices.Where(i => !candidates.Contains(i)).First(i => !DiagonallyAdjacent(candidates, i));
                candidates.Add(index);
            }
            foreach (var index in candidates) {
                mat[index] = true;
            }
            return new(sector, mat, makeRoom, nBestCorridors);
        }

        static Coord ToCoord(int a) => new(a % 4, a / 4);

        public static bool CardinallyAdjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            var x = candidates.Select(c => ToCoord(c))
                .ToList();
            return x
                .Any(q => p.DistSq(q) == 1);
        }

        public static bool DiagonallyAdjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            return candidates.Select(c => ToCoord(c))
                .Any(q => p.DistSq(q) == 2);
        }

        public void Draw(FloorGenerationContext ctx)
        {
            foreach (var room in Rooms) {
                ctx.Draw(room);
            }
            foreach (var corridor in Corridors) {
                ctx.Draw(corridor);
            }
        }
    }
}
