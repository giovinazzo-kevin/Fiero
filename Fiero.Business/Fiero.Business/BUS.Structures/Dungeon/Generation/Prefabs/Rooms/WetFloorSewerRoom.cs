namespace Fiero.Business
{
    public class WetFloorSewerRoom : Room
    {
        protected override DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            theme = base.CustomizeTheme(theme);
            return theme with
            {
                WaterTile = c => theme.WaterTile(c).WithCustomColor(ColorName.LightGreen)
            };
        }

        protected override void DrawRects(FloorGenerationContext ctx)
        {
            var points = Rects
                .SelectMany(r => Shapes.Rect(r.Position(), r.Size()))
                .Select(p => new SimplexNumber2D(p.X, p.Y, scale: 0.15f, range: 2, rng: Rng.Noise))
                ;
            foreach (var simplex in points)
            {
                switch (simplex.Next())
                {
                    case 0:
                        ctx.Draw(new(simplex.X, simplex.Y), Theme.WaterTile);
                        break;
                    case 1:
                        ctx.Draw(new(simplex.X, simplex.Y), Theme.RoomTile);
                        break;
                }
            }
        }
    }
}
