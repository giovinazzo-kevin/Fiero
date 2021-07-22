using Fiero.Core;

namespace Fiero.Business
{
    public class DungeonBranchGenerator : DungeonGenerator
    {
        public override Floor GenerateFloor(FloorId floorId, FloorBuilder builder)
        {
            var center = builder.Size / 2;
            return builder
                .WithStep(ctx => {
                    var radius = (int)builder.Size.ToVec().Magnitude() / 4;
                    ctx.FillBox(new(), builder.Size, TileName.Wall);
                    ctx.FillCircle(center, radius, TileName.Ground);
                    ctx.AddObject(DungeonObjectName.Upstairs, center);
                })
                .Build(floorId);
        }
    }
}
