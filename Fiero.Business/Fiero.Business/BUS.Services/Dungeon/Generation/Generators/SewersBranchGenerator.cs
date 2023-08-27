using Fiero.Core;
using System;

namespace Fiero.Business
{

    [TransientDependency]
    public class SewersBranchGenerator : RoomTreeGenerator
    {
        public static readonly DungeonTheme DefaultTheme = DungeonTheme.Default with
        {
            WallTile = (c => DungeonTheme.Default.WallTile(c).WithCustomColor(ColorName.LightGreen)),
            RoomTile = (c => DungeonTheme.Default.RoomTile(c).WithCustomColor(ColorName.Gray)),
            CorridorTile = (c => DungeonTheme.Default.CorridorTile(c).WithCustomColor(ColorName.Green)),
            RoomSquares = new(3, 8, (d, s) => 1f / Math.Pow(s, 2)),
        };

        public SewersBranchGenerator() : base(DefaultTheme) { }
        public override Coord MapSize(FloorId id) => new(50, 50);
        public override Coord GridSize(FloorId id) => new(2, 2);
        protected override PoolBuilder<Func<Room>> ConfigureRoomPool(PoolBuilder<Func<Room>> pool) => pool
            .Include(() => new EmptyRoom(), 1)
            .Guarantee(() => new TreasureRoom(), minAmount: 1);
        protected override void OnRoomDrawn(Room room, FloorGenerationContext ctx)
        {

        }
    }
}
