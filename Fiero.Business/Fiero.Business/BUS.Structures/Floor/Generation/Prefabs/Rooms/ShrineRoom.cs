using Fiero.Core;
using System;
using System.Linq;
using System.Threading;

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
            var pos = this.Rects.Shuffle(Rng.Random).First().Center();
            ctx.AddObject(nameof(FeatureName.Shrine), pos, e => e.Feature_Shrine());
        }
    }
}
