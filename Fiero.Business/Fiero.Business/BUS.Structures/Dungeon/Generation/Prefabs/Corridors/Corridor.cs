using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{

    public class Corridor : IFloorGenerationPrefab
    {
        public RoomConnector Start { get; private set; }
        public RoomConnector End { get; private set; }
        public ColorName Color { get; set; }

        public readonly Coord[] Points;

        protected virtual TileDef WallTile(Coord c) => new(TileName.Wall, c);
        protected virtual TileDef GroundTile(Coord c) => new(TileName.Corridor, c, Color);
        protected virtual EntityBuilder<Feature> DoorFeature(GameEntityBuilders e, Coord c) => e.Feature_Door();
        protected virtual Chance DoorChance() => new(1, 3);

        public Corridor(RoomConnector a, RoomConnector b, ColorName color)
        {
            Start = a;
            End = b;
            Color = color;
            Points = Generate().ToArray();
        }

        IEnumerable<Coord> Generate()
        {
            var startMiddle = (Start.Edge.Left + Start.Edge.Right) / 2;
            var endMiddle = (End.Edge.Left + End.Edge.Right) / 2;
            var v1 = (Start.Edge.Left - Start.Edge.Right).Clamp(-1, 1);
            var d1 = (int)(Math.Atan2(v1.Y, v1.X) * 180f / Math.PI);
            var v2 = (End.Edge.Left - End.Edge.Right).Clamp(-1, 1);
            var d2 = (int)(Math.Atan2(v2.Y, v2.X) * 180f / Math.PI);
            var connectStart = startMiddle;
            var connectEnd = endMiddle;
            var middle = (startMiddle + endMiddle) / 2;
            var l1 = new Line(startMiddle, startMiddle + new Coord(v1.Y, v1.X));
            var l2 = new Line(endMiddle, endMiddle + new Coord(v2.Y, v2.X));
            if (!l1.IsParallel(l2))
            {
                middle = new(
                    (l1.B * l2.C - l2.B * l1.C) / (l1.A * l2.B - l2.A * l1.B),
                    (l1.C * l2.A - l2.C * l1.A) / (l1.A * l2.B - l2.A * l1.B)
                );
            }
            switch (d1.Mod(360))
            {
                case 0:
                case 180:
                    foreach (var p in Shapes.Line(startMiddle, connectStart = new(startMiddle.X, middle.Y))) yield return p;
                    break;
                case 90:
                case 270:
                    foreach (var p in Shapes.Line(startMiddle, connectStart = new(middle.X, startMiddle.Y))) yield return p;
                    break;
            }
            switch (d2.Mod(360))
            {
                case 0:
                case 180:
                    foreach (var p in Shapes.Line(endMiddle, connectEnd = new(endMiddle.X, middle.Y))) yield return p;
                    break;
                case 90:
                case 270:
                    foreach (var p in Shapes.Line(endMiddle, connectEnd = new(middle.X, endMiddle.Y))) yield return p;
                    break;
            }
            foreach (var p in Shapes.Line(connectStart, connectEnd)) yield return p;
        }

        public virtual void Draw(FloorGenerationContext ctx)
        {
            foreach (var p in Points)
            {
                ctx.Draw(p, GroundTile);
            }
            var startMiddle = (Start.Edge.Left + Start.Edge.Right) / 2;
            var endMiddle = (End.Edge.Left + End.Edge.Right) / 2;
            if (DoorChance().Check() && !ctx.GetObjects().Any(obj => obj.Position == startMiddle))
            {
                ctx.TryAddFeature(nameof(FeatureName.Door), startMiddle, e => DoorFeature(e, startMiddle));
            }
            if (DoorChance().Check() && !ctx.GetObjects().Any(obj => obj.Position == endMiddle))
            {
                ctx.TryAddFeature(nameof(FeatureName.Door), endMiddle, e => DoorFeature(e, endMiddle));
            }
        }
    }
}
