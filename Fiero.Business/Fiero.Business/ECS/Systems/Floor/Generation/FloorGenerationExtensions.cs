using Fiero.Core;
using System;

namespace Fiero.Business
{

    public static class FloorGenerationExtensions
    {
        public static void DrawLine(this FloorGenerationContext ctx, Coord start, Coord end, TileName tile)
        {
            foreach (var p in Shape.Line(start, end)) {
                ctx.SetTile(p.X, p.Y, tile);
            }
        }

        public static void DrawCircle(this FloorGenerationContext ctx, Coord center, int radius, TileName tile)
        {
            foreach (var p in Shape.Circle(center, radius)) {
                ctx.SetTile(p.X, p.Y, tile);
            }
        }

        public static void DrawBox(this FloorGenerationContext ctx, Coord topLeft, Coord size, TileName tile)
        {
            size = new(topLeft.X + size.X, topLeft.Y + size.Y);
            ctx.DrawLine(topLeft, new(size.X - 1, topLeft.Y), tile);
            ctx.DrawLine(topLeft, new(topLeft.X, size.Y - 1), tile);
            ctx.DrawLine(new(size.X - 1, topLeft.Y), new(size.X - 1, size.Y - 1), tile);
            ctx.DrawLine(new(topLeft.X, size.Y - 1), new(size.X - 1, size.Y - 1), tile);
        }

        public static void FillBox(this FloorGenerationContext ctx, Coord topLeft, Coord size, TileName tile)
        {
            for (int x = 0; x < size.X; x++) {
                for (int y = 0; y < size.Y; y++) {
                    ctx.SetTile(x + topLeft.X, y + topLeft.Y, tile);
                }
            }
        }

        public static void FillCircle(this FloorGenerationContext ctx, Coord center, int radius, TileName tile)
        {
            for (var x = center.X - radius; x <= center.X; x++) {
                for (var y = center.Y - radius; y <= center.Y; y++) {
                    // we don't have to take the square root, it's slow
                    if ((x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) <= radius * radius) {
                        var xSym = center.X - (x - center.X);
                        var ySym = center.Y - (y - center.Y);
                        // (x, y), (x, ySym), (xSym , y), (xSym, ySym) are in the circle
                        ctx.SetTile(x, y, tile);
                        ctx.SetTile(x, ySym, tile);
                        ctx.SetTile(xSym, y, tile);
                        ctx.SetTile(xSym, ySym, tile);
                    }
                }
            }
        }
    }
}
