using Fiero.Core;
using System.Linq;

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

        protected override TileDef WallTile(Coord c) => base.WallTile(c).WithCustomColor(ColorName.Yellow);
        protected override TileDef GroundTile(Coord c) => base.GroundTile(c).WithCustomColor(ColorName.LightYellow);
        protected virtual TileDef GroundTileAlt(Coord c) => base.GroundTile(c).WithCustomColor(ColorName.LightGray);

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = Rects.Shuffle(Rng.Random).First().Center();
            if (!ctx.TryAddFeature(nameof(FeatureName.Chest), Shapes.SquareSpiral(pos, 24), e => e.Feature_Chest(), out _))
                ctx.Log.Write($"Could not add {nameof(FeatureName.Chest)} near {pos}!");
        }
    }
}
