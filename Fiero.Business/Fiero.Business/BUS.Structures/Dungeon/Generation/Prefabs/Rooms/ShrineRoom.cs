using Fiero.Core;
using Fiero.Core.Extensions;
using System.Linq;

namespace Fiero.Business
{
    public class ShrineRoom : Room
    {
        public ShrineRoom()
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
                WallTile = (c => theme.WallTile(c).WithCustomColor(ColorName.White)),
                RoomTile = (c => theme.RoomTile(c).WithCustomColor(ColorName.White)),
            };
        }

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = Rects.Shuffle(Rng.Random).First().Center();
            if (!ctx.TryAddFeature(nameof(FeatureName.Shrine), Shapes.SquareSpiral(pos, 24), e => e.Feature_Shrine(), out _))
                ctx.Log.Write($"Could not add {nameof(FeatureName.Shrine)} near {pos}!");
        }
    }
}
