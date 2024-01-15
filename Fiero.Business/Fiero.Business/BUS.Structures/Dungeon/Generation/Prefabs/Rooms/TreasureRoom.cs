namespace Fiero.Business
{
    public class TreasureRoom : Room
    {
        public TreasureRoom()
        {
            AllowMonsters = false;
            AllowFeatures = false;
            AllowItems = false;
        }
        protected override DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            theme = base.CustomizeTheme(theme);
            return theme with
            {
                WallTile = (c => theme.WallTile(c).WithCustomColor(ColorName.Yellow))
            };
        }

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = Rects.Shuffle(Rng.Random).First().Center();
            if (!ctx.TryAddFeature(nameof(FeatureName.Chest), Shapes.SquareSpiral(pos, 24), e => e.Feature_Chest(), out _))
                ctx.Log.Write($"Could not add {nameof(FeatureName.Chest)} near {pos}!");
        }
    }
}
