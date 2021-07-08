using Fiero.Core;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{

    public static class FloorGenerationExtensions
    {
        public static void DrawLine(this FloorGenerationContext ctx, (int X, int Y) start, (int X, int Y) end, TileName tile) 
        {
            Utils.Bresenham(start, end, (x, y) => { ctx.Set(x, y, tile); return true; });
        }

        public static void DrawBox(this FloorGenerationContext ctx, (int X, int Y) topLeft, (int Width, int Height) size, TileName tile)
        {
            size = (topLeft.X + size.Width, topLeft.Y + size.Height);

            ctx.DrawLine(topLeft, (size.Width - 1, topLeft.Y), tile);
            ctx.DrawLine(topLeft, (topLeft.X, size.Height - 1), tile);
            ctx.DrawLine((size.Width - 1, topLeft.Y), (size.Width - 1, size.Height - 1), tile);
            ctx.DrawLine((topLeft.X, size.Height - 1), (size.Width - 1, size.Height - 1), tile);
        }

        public static void DrawRoom(this FloorGenerationContext ctx, 
            (int X, int Y) topLeft, 
            (int Width, int Height) size, 
            (bool L, bool R, bool U, bool D) doors,
            TileName wall = TileName.Wall,
            TileName door = TileName.Door,
            bool isCorridor = false,
            bool hasRoundedCorners = false)
        {
            for (var x = topLeft.X + 1; x < topLeft.X + size.Width - 1; x++) {
                for (var y = topLeft.Y + 1; y < topLeft.Y + size.Height - 1; y++) {
                    ctx.Set(x, y, isCorridor ? TileName.Wall : TileName.Ground);
                }
            }

            ctx.DrawBox(topLeft, size, wall);
            var middle = (topLeft.X + size.Width / 2, topLeft.Y + size.Height / 2);
            if (doors.L) {
                var p = (X: topLeft.X, Y: topLeft.Y + (size.Height - 1) / 2);
                if(isCorridor) ctx.DrawLine(p, middle, TileName.Ground);
                ctx.Set(p.X, p.Y, door);
            }
            if (doors.R) {
                var p = (X: topLeft.X + size.Width - 1, Y: topLeft.Y + (size.Height - 1) / 2);
                if(isCorridor) ctx.DrawLine(p, middle, TileName.Ground);
                ctx.Set(p.X, p.Y, door);
            }
            if (doors.U) {
                var p = (X: topLeft.X + (size.Width - 1) / 2, Y: topLeft.Y);
                if (isCorridor) ctx.DrawLine(p, middle, TileName.Ground);
                ctx.Set(p.X, p.Y, door);
            }
            if (doors.D) {
                var p = (X: topLeft.X + (size.Width - 1) / 2, Y: topLeft.Y + size.Height - 1);
                if (isCorridor) ctx.DrawLine(p, middle, TileName.Ground);
                ctx.Set(p.X, p.Y, door);
            }
            if(hasRoundedCorners) {
                ctx.Set(topLeft.X + 1, topLeft.Y + 1, wall);
                ctx.Set(topLeft.X + size.Width - 3 + 1, topLeft.Y + 1, wall);
                ctx.Set(topLeft.X + 1, topLeft.Y + size.Height - 3 + 1, wall);
                ctx.Set(topLeft.X + size.Width - 3 + 1, topLeft.Y + size.Height - 3 + 1, wall);
            }
        }

        public static void DrawDungeon(this FloorGenerationContext ctx, Dungeon dungeon)
        {
            var viewportSize = new Coord((int)(0.75 * ctx.Size.X), (int)(0.75 * ctx.Size.Y));
            var viewportOffset = new Coord((ctx.Size.X - viewportSize.X) / 2, (ctx.Size.Y - viewportSize.Y) / 2);

            var dungeonBoundsInRooms = new Coord(
                dungeon.Nodes.Max(n => n.Position.X) - dungeon.Nodes.Min(n => n.Position.X) + 1,
                dungeon.Nodes.Max(n => n.Position.Y) - dungeon.Nodes.Min(n => n.Position.Y) + 1
            );
            var roomSizeInTiles = new Coord(
                11, 11 
            );
            var startNode = dungeon.Nodes.Single(n => n.Type == DungeonNodeType.Start);
            var geometricCenter = new Coord(
                roomSizeInTiles.X * dungeon.Nodes.Sum(n => n.Position.X) / dungeon.Nodes.Count,
                roomSizeInTiles.Y * dungeon.Nodes.Sum(n => n.Position.Y) / dungeon.Nodes.Count
            ); 
            var origin = new Coord(
                 viewportSize.X / 2 - roomSizeInTiles.X / 2 - geometricCenter.X,
                 viewportSize.Y / 2 - roomSizeInTiles.Y / 2 - geometricCenter.Y
            );
            var secretRooms = dungeon.Nodes.Where(n => n.Type == DungeonNodeType.Secret);
            var visited = new HashSet<DungeonGenerationNode>();
            Draw(startNode, visited);
            foreach (var secret in secretRooms) {
                Draw(secret, visited);
            }
            // In order to create more interesting layouts, we can delete some walls between adjacent, non-special rooms of the same type
            foreach (var node in dungeon.Nodes) {
                if (node.Type != DungeonNodeType.Normal && node.Type != DungeonNodeType.Enemies)
                    continue;
                var roomTopLeft = RoomTopLeft(node);
                // Special case: 2x2 clusters are always merged!
                if (node.South?.Type == node.Type && node.East?.Type == node.Type && node.South.East?.Type == node.Type) {
                    var oppositeRoomTopLeft = RoomTopLeft(node.South.East);
                    MergeSouth(node, roomTopLeft);
                    MergeEast(node, roomTopLeft);
                    MergeWest(node.South.East, oppositeRoomTopLeft);
                    MergeNorth(node.South.East, oppositeRoomTopLeft);
                    ctx.Set(roomTopLeft.X + roomSizeInTiles.X - 1, roomTopLeft.Y + roomSizeInTiles.Y - 1, TileName.Ground);
                    ctx.Set(roomTopLeft.X + roomSizeInTiles.X, roomTopLeft.Y + roomSizeInTiles.Y - 1, TileName.Ground);
                    ctx.Set(roomTopLeft.X + roomSizeInTiles.X - 1, roomTopLeft.Y + roomSizeInTiles.Y, TileName.Ground);
                    ctx.Set(roomTopLeft.X + roomSizeInTiles.X, roomTopLeft.Y + roomSizeInTiles.Y, TileName.Ground);
                }
                else {
                    if (node.North?.Type == node.Type && Rng.Random.NextDouble() < dungeon.GenerationSettings.RoomMergeChance) {
                        MergeNorth(node, roomTopLeft);
                    }
                    if (node.South?.Type == node.Type && Rng.Random.NextDouble() < dungeon.GenerationSettings.RoomMergeChance) {
                        MergeSouth(node, roomTopLeft);
                    }
                    if (node.West?.Type == node.Type && Rng.Random.NextDouble() < dungeon.GenerationSettings.RoomMergeChance) {
                        MergeWest(node, roomTopLeft);
                    }
                    if (node.East?.Type == node.Type && Rng.Random.NextDouble() < dungeon.GenerationSettings.RoomMergeChance) {
                        MergeEast(node, roomTopLeft);
                    }
                }

                Coord RoomTopLeft(DungeonGenerationNode node)
                {
                    var roomTopLeftRelativeToOrigin = new Coord(node.Position.X * roomSizeInTiles.X, node.Position.Y * roomSizeInTiles.Y);
                    return new Coord(roomTopLeftRelativeToOrigin.X + origin.X + viewportOffset.X, roomTopLeftRelativeToOrigin.Y + origin.Y + viewportOffset.Y);
                }

                void MergeNorth(DungeonGenerationNode node, Coord roomTopLeft)
                {
                    ctx.DrawLine((roomTopLeft.X + 1, roomTopLeft.Y), (roomTopLeft.X + roomSizeInTiles.X - 2, roomTopLeft.Y), TileName.Ground);
                    ctx.DrawLine((roomTopLeft.X + 1, roomTopLeft.Y - 1), (roomTopLeft.X + roomSizeInTiles.X - 2, roomTopLeft.Y - 1), TileName.Ground);
                }
                void MergeSouth(DungeonGenerationNode node, Coord roomTopLeft)
                {
                    ctx.DrawLine((roomTopLeft.X + 1, roomTopLeft.Y + roomSizeInTiles.Y - 0), (roomTopLeft.X + roomSizeInTiles.X - 2, roomTopLeft.Y + roomSizeInTiles.Y - 0), TileName.Ground);
                    ctx.DrawLine((roomTopLeft.X + 1, roomTopLeft.Y + roomSizeInTiles.Y - 1), (roomTopLeft.X + roomSizeInTiles.X - 2, roomTopLeft.Y + roomSizeInTiles.Y - 1), TileName.Ground);
                }
                void MergeWest(DungeonGenerationNode node, Coord roomTopLeft)
                {
                    ctx.DrawLine((roomTopLeft.X, roomTopLeft.Y + 1), (roomTopLeft.X, roomTopLeft.Y + roomSizeInTiles.Y - 2), TileName.Ground);
                    ctx.DrawLine((roomTopLeft.X - 1, roomTopLeft.Y + 1), (roomTopLeft.X - 1, roomTopLeft.Y + roomSizeInTiles.Y - 2), TileName.Ground);
                }
                void MergeEast(DungeonGenerationNode node, Coord roomTopLeft)
                {
                    ctx.DrawLine((roomTopLeft.X + roomSizeInTiles.X - 0, roomTopLeft.Y + 1), (roomTopLeft.X + roomSizeInTiles.X - 0, roomTopLeft.Y + roomSizeInTiles.Y - 2), TileName.Ground);
                    ctx.DrawLine((roomTopLeft.X + roomSizeInTiles.X - 1, roomTopLeft.Y + 1), (roomTopLeft.X + roomSizeInTiles.X - 1, roomTopLeft.Y + roomSizeInTiles.Y - 2), TileName.Ground);
                }
            }
            void Draw(DungeonGenerationNode node, HashSet<DungeonGenerationNode> visited = null)
            {
                visited ??= new HashSet<DungeonGenerationNode>();
                var roomTopLeftRelativeToOrigin = new Coord(
                    node.Position.X * roomSizeInTiles.X,
                    node.Position.Y * roomSizeInTiles.Y
                );
                var roomTopLeft = new Coord(
                    roomTopLeftRelativeToOrigin.X + origin.X + viewportOffset.X,
                    roomTopLeftRelativeToOrigin.Y + origin.Y + viewportOffset.Y
                );
                var wallTile = TileName.Wall;
                var doorTile = node.Type switch {
                    DungeonNodeType.Start => TileName.Door,
                    DungeonNodeType.Item => TileName.Door,
                    DungeonNodeType.Shop => TileName.Door,
                    DungeonNodeType.Boss => TileName.Door,
                    DungeonNodeType.Secret => TileName.Wall,
                    DungeonNodeType.Corridor => TileName.Ground,
                    _ => TileName.Ground
                };
                var isCorridor = node.Type == DungeonNodeType.Corridor;
                var hasRoundedCorners = node.Type == DungeonNodeType.Boss || node.Type == DungeonNodeType.Shop
                    || node.Type == DungeonNodeType.Item || node.Type == DungeonNodeType.Secret;
                var doors = (node.West != null, node.East != null, node.North != null, node.South != null);
                var openSet = new List<Coord>();
                ctx.DrawRoom(
                    (roomTopLeft.X, roomTopLeft.Y), 
                    (roomSizeInTiles.X, roomSizeInTiles.Y), 
                    doors, wallTile, doorTile, isCorridor, hasRoundedCorners);
                for (var y = roomTopLeft.Y; y < roomTopLeft.Y + roomSizeInTiles.Y; y++) {
                    for (var x = roomTopLeft.X; x < roomTopLeft.X + roomSizeInTiles.X; x++) {
                        if(ctx.Get(x, y) == TileName.Ground) {
                            openSet.Add(new Coord(x, y));
                        }
                    }
                }
                foreach (var obj in node.Objects.OrderBy(GetPriority)) {
                    if(!TryGetObjectPosition(obj, out var pos)) {
                        break;
                    }
                    ctx.Add(obj, pos);
                }
                visited.Add(node);
                if (!visited.Contains(node.North) && node.North != null) {
                    Draw(node.North, visited);
                }
                if (!visited.Contains(node.South) && node.South != null) {
                    Draw(node.South, visited);
                }
                if (!visited.Contains(node.East) && node.East != null) {
                    Draw(node.East, visited);
                }
                if (!visited.Contains(node.West) && node.West != null) {
                    Draw(node.West, visited);
                }

                int GetPriority(DungeonObjectName type) => type switch {
                    DungeonObjectName.Upstairs => -1000,
                    DungeonObjectName.Downstairs => -1000,
                    DungeonObjectName.Shrine => -100,
                    DungeonObjectName.Boss => -100,
                    DungeonObjectName.Chest => -50,
                    DungeonObjectName.Item => -40,
                    DungeonObjectName.ItemForSale => -40,
                    DungeonObjectName.ConsumableForSale => -30,
                    DungeonObjectName.Consumable => -10,
                    DungeonObjectName.Trap => -1,
                    DungeonObjectName.Enemy => 0,
                    _ => 0
                };

                bool TryGetObjectPosition(DungeonObjectName type, out Coord pos)
                {
                    pos = default;
                    if(!openSet.Any()) {
                        return false;
                    }
                    pos = openSet[Rng.Random.Next(openSet.Count)];
                    // These are always placed in the middle of the room
                    if (type == DungeonObjectName.Boss
                    || type == DungeonObjectName.Shrine
                    || type == DungeonObjectName.Upstairs
                    || type == DungeonObjectName.Downstairs) {
                        pos = openSet
                            .OrderBy(x => x.Dist(new(roomTopLeft.X + roomSizeInTiles.X / 2, roomTopLeft.Y + roomSizeInTiles.Y / 2)))
                            .First();
                    }
                    // These are always placed on the corners of the room
                    if (type == DungeonObjectName.Enemy) {
                        pos = openSet
                            .OrderByDescending(x => x.Dist(new(roomTopLeft.X + roomSizeInTiles.X / 2, roomTopLeft.Y + roomSizeInTiles.Y / 2)))
                            .First();
                    }
                    // These are always placed in the middle-ish of the room and evenly spaced apart
                    if (type == DungeonObjectName.Item
                    || type == DungeonObjectName.ItemForSale
                    || type == DungeonObjectName.ConsumableForSale) {
                        pos = openSet
                            .Where(x => (x.X + x.Y) % 2 == 0)
                            .OrderBy(x => x.Dist(new(roomTopLeft.X + roomSizeInTiles.X / 2, roomTopLeft.Y + roomSizeInTiles.Y / 2)))
                            .First();
                    }
                    openSet.Remove(pos);
                    return true;
                }
            }
        }
    }
}
