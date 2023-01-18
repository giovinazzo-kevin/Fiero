using Fiero.Core;
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

        protected override TileDef WallTile(Coord c) => base.WallTile(c).WithCustomColor(ColorName.White);
        protected override TileDef GroundTile(Coord c) => base.GroundTile(c).WithCustomColor(ColorName.White);

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = Rects.Shuffle(Rng.Random).First().Center();
            if (!ctx.TryAddFeature(nameof(FeatureName.Shrine), Shapes.SquareSpiral(pos, 24), e => e.Feature_Shrine(), out _))
                ctx.Log.Write($"Could not add {nameof(FeatureName.Shrine)} near {pos}!");
        }
    }
}
