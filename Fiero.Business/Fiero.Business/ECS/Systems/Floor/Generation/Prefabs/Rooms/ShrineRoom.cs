using Fiero.Core;
using System.Linq;
using System.Threading;

namespace Fiero.Business
{
    public class ShrineRoom : Room
    {
        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var shrinePos = this.Rects.Shuffle(Rng.Random).First().Center();
            ctx.AddObject(DungeonObjectName.Shrine, shrinePos);
        }
    }
}
