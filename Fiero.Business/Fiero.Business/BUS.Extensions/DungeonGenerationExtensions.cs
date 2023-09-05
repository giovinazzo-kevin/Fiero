using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public readonly record struct FloorGenerationRule(Func<FloorGenerationContext, TileDef, bool> Match, Func<FloorGenerationContext, TileDef, TileDef> Apply);

    public static class DungeonGenerationExtensions
    {
        public static void Draw(this FloorGenerationContext ctx, Coord p, Func<Coord, TileDef> makeTile)
        {
            var tile = makeTile(p);
            ctx.SetTile(p, tile);
        }

        public static void DrawLine(this FloorGenerationContext ctx, Coord start, Coord end, Func<Coord, TileDef> makeTile)
        {
            foreach (var p in Shapes.Line(start, end))
            {
                var tile = makeTile(p);
                ctx.SetTile(p, tile);
            }
        }

        public static void DrawCircle(this FloorGenerationContext ctx, Coord center, int radius, Func<Coord, TileDef> makeTile)
        {
            foreach (var p in Shapes.Circle(center, radius))
            {
                var tile = makeTile(p);
                ctx.SetTile(p, tile);
            }
        }

        public static void DrawBox(this FloorGenerationContext ctx, Coord topLeft, Coord size, Func<Coord, TileDef> makeTile)
        {
            size = new(topLeft.X + size.X, topLeft.Y + size.Y);
            ctx.DrawLine(topLeft, new(size.X - 1, topLeft.Y), makeTile);
            ctx.DrawLine(topLeft, new(topLeft.X, size.Y - 1), makeTile);
            ctx.DrawLine(new(size.X - 1, topLeft.Y), new(size.X - 1, size.Y - 1), makeTile);
            ctx.DrawLine(new(topLeft.X, size.Y - 1), new(size.X - 1, size.Y - 1), makeTile);
        }

        public static void FillBox(this FloorGenerationContext ctx, Coord topLeft, Coord size, Func<Coord, TileDef> makeTile)
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    var p = new Coord(x, y);
                    var tile = makeTile(p);
                    ctx.SetTile(p + topLeft, tile);
                }
            }
        }

        public static void FillCircle(this FloorGenerationContext ctx, Coord center, int radius, Func<Coord, TileDef> makeTile)
        {
            for (var x = center.X - radius; x <= center.X; x++)
            {
                for (var y = center.Y - radius; y <= center.Y; y++)
                {
                    // we don't have to take the square root, it's slow
                    if ((x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) <= radius * radius)
                    {
                        var xSym = center.X - (x - center.X);
                        var ySym = center.Y - (y - center.Y);
                        // (x, y), (x, ySym), (xSym , y), (xSym, ySym) are in the circle
                        SetTile(new(x, y));
                        SetTile(new(x, ySym));
                        SetTile(new(xSym, y));
                        SetTile(new(xSym, ySym));
                    }
                }
            }

            void SetTile(Coord xy)
            {
                var tile = makeTile(xy);
                ctx.SetTile(xy, tile);
            }
        }

        public static void Draw(this FloorGenerationContext ctx, IFloorGenerationPrefab r) => r.Draw(ctx);
        public static void Rule(this FloorGenerationContext ctx, FloorGenerationRule rule)
        {
            foreach (var tile in ctx.GetTiles()
                .Where(x => rule.Match(ctx, x))
                .ToArray() // Force computation to run in two separate steps
                .Select(x => rule.Apply(ctx, x)))
                ctx.SetTile(tile.Position, tile);
        }
    }
}
