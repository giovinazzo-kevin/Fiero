using Fiero.Core;
using Fiero.Core.Structures;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class CrampedRoom : Room
    {
        public CrampedRoom()
        {
            AllowMonsters = false;
            AllowFeatures = false;
            AllowItems = false;
        }

        protected override TileDef WallTile(Coord c) => base.WallTile(c).WithCustomColor(ColorName.Red);
        protected override TileDef GroundTile(Coord c) => base.GroundTile(c).WithCustomColor(ColorName.LightGray);

        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            foreach (var rect in Rects)
                ctx.FillBox(rect.Position(), rect.Size(), WallTile);
            var usedConnectors = SelectConnectors().Where(x => x.IsActive).ToArray();
            var pairs = usedConnectors
                .SelectMany(a => usedConnectors
                    .Select(b => new UnorderedPair<RoomConnector>(a, b)))
                .Where(p => p.Left.Middle != p.Right.Middle)
                .Distinct();
            foreach (var p in pairs)
            {
                Console.WriteLine(p);
                new Corridor(p.Left, p.Right, ColorName.Red)
                    .Draw(ctx);
            }
        }
    }
}
