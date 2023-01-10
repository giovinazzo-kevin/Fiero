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

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = this.Rects.Shuffle(Rng.Random).First().Center();
            ctx.AddObject(nameof(FeatureName.Chest), pos, e => e.Feature_Chest());
        }
    }
}
