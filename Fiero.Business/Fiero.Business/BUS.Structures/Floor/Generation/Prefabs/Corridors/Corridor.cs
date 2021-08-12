using Fiero.Core;
using System;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class Corridor : IFloorGenerationPrefab
    {

        public UnorderedPair<Coord> Start { get; set; }
        public UnorderedPair<Coord> End { get; set; }
        public int Thickness { get; set; }

        protected virtual TileDef WallTile(Coord c) => new(TileName.Wall, c);
        protected virtual TileDef GroundTile(Coord c) => new(TileName.Corridor, c);

        public Corridor(UnorderedPair<Coord> a, UnorderedPair<Coord> b, int thickness = 1)
        {
            Start = a;
            End = b;
            Thickness = thickness;
        }

        public virtual void Draw(FloorGenerationContext ctx)
        {
            var startMiddle = (Start.Left + Start.Right) / 2;
            var endMiddle = (End.Left + End.Right) / 2;
            var v1 = (Start.Left - Start.Right).Clamp(-1, 1);
            var d1 = (int)(Math.Atan2(v1.Y, v1.X) * 180f / Math.PI);
            var v2 = (End.Left - End.Right).Clamp(-1, 1);
            var d2 = (int)(Math.Atan2(v2.Y, v2.X) * 180f / Math.PI);
            var connectStart = startMiddle;
            var connectEnd = endMiddle;
            var middle = (startMiddle + endMiddle) / 2;
            switch (d1.Mod(360)) {
                case 0:
                case 180:
                    ctx.DrawLine(startMiddle, connectStart = new(startMiddle.X, middle.Y), GroundTile);
                    break;
                case 90:
                case 270:
                    ctx.DrawLine(startMiddle, connectStart = new(middle.X, startMiddle.Y), GroundTile);
                    break;
            }
            switch (d2.Mod(360)) {
                case 0:
                case 180:
                    ctx.DrawLine(endMiddle, connectEnd = new(endMiddle.X, middle.Y), GroundTile);
                    break;
                case 90:
                case 270:
                    ctx.DrawLine(endMiddle, connectEnd = new(middle.X, endMiddle.Y), GroundTile);
                    break;
            }
            ctx.DrawLine(connectStart, connectEnd, GroundTile);
            if (Rng.Random.OneChanceIn(3) && !ctx.GetObjects().Any(obj => obj.Position == startMiddle)) {
                ctx.AddObject(DungeonObjectName.Door, startMiddle);
            }
            if (Rng.Random.OneChanceIn(3) && !ctx.GetObjects().Any(obj => obj.Position == endMiddle)) {
                ctx.AddObject(DungeonObjectName.Door, endMiddle);
            }
        }
    }
}
