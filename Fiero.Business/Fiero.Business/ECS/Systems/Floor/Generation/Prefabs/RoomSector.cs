using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public readonly struct RoomSector : IFloorGenerationPrefab
    {
        public readonly bool[] Cells;
        public readonly IntRect Sector;
        public readonly Room[] Rooms;
        public readonly Corridor[] Corridors;

        public RoomSector(IntRect sector, bool[] cells, Func<Room> makeRoom)
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
                        room.AddRect(rects[i]);
                    }
                    return room;
                })
                .ToArray();

            var corridors = new List<Corridor>();
            for (int i = 1; i < Rooms.Length; i++) {
                var prev = Rooms[i - 1];
                var pairs = Rooms[i].GetConnectors()
                    .SelectMany(c => prev.GetConnectors().Select(d => new UnorderedPair<UnorderedPair<Coord>>(c, d)));
                var bestPair = pairs
                    .Where(p => new Line(p.Left.Left, p.Left.Right).IsParallel(new Line(p.Right.Left, p.Right.Right)))
                    .OrderBy(p => ((p.Left.Left + p.Left.Right) / 2).DistSq((p.Right.Left + p.Right.Right) / 2))
                    .First();
                corridors.Add(new(bestPair.Left, bestPair.Right));
            }
            Corridors = corridors.ToArray();

            Console.WriteLine($"Rooms: {Rooms.Length}; Corridors: {Corridors.Length}");
        }

        public static IEnumerable<Corridor> GenerateInterSectorCorridors(IEnumerable<RoomSector> sectors)
        {
            var ret = new List<Corridor>();
            var roomSectors = sectors.Select((s, i) => (Index: i, Sector: s)).ToList();
            for (int i = 0; i < roomSectors.Count; i++) {
                var neighbors = roomSectors.Where(x => 
                    CardinallyAdjacent(new[] { x.Index }, roomSectors[i].Index))
                    .ToList();
                // Make a connection towards 1 to 3 neighbors
                var connections = Rng.Random.Between(1, 3);
                for (int j = 0; j < connections && neighbors.Count > 0; j++) {
                    var n = Rng.Random.Next(neighbors.Count);
                    var neighbor = neighbors[n];
                    neighbors.RemoveAt(n);

                    var pairs = roomSectors[i].Sector.Rooms.SelectMany(r => r.GetConnectors())
                        .SelectMany(c => neighbor.Sector.Rooms.SelectMany(r => r.GetConnectors())
                            .Select(d => new UnorderedPair<UnorderedPair<Coord>>(c, d)));
                    var bestPair = pairs
                        .Where(p => new Line(p.Left.Left, p.Left.Right).IsParallel(new Line(p.Right.Left, p.Right.Right)))
                        .OrderBy(p => ((p.Left.Left + p.Left.Right) / 2).DistSq((p.Right.Left + p.Right.Right) / 2))
                        .First();
                    ret.Add(new(bestPair.Left, bestPair.Right));
                }
            }
            return ret;
        }

        public static RoomSector Create(IntRect sector, Func<Room> makeRoom)
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
            return new(sector, mat, makeRoom);
        }

        static Coord ToCoord(int a) => new(a % 4, a / 4);

        public static bool CardinallyAdjacent(IEnumerable<int> candidates, int a)
        {
            var p = ToCoord(a);
            return candidates.Select(c => ToCoord(c))
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
