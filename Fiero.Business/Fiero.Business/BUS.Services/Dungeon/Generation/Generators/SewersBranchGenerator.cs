namespace Fiero.Business
{

    [TransientDependency]
    public class SewersBranchGenerator : RoomTreeGenerator
    {
        public static readonly DungeonTheme DefaultTheme = DungeonTheme.Default with
        {
            WallTile = (c => DungeonTheme.Default.WallTile(c).WithCustomColor(ColorName.Gray)),
            RoomTile = (c => DungeonTheme.Default.RoomTile(c).WithCustomColor(ColorName.Gray)),
            CorridorTile = (c => DungeonTheme.Default.CorridorTile(c).WithCustomColor(ColorName.LightGreen)),
            RoomSquares = new(2, 4, (d, s) => 1f / Math.Pow(s, 2)),
        };

        public SewersBranchGenerator() : base(DefaultTheme) { }
        public override Coord MapSize(FloorId id) => id.Depth switch
        {
            1 => new(25, 25),
            _ => new(50, 50)
        };
        public override Coord GridSize(FloorId id) => id.Depth switch
        {
            1 => new(1, 1),
            _ => new(2, 2)
        };
        protected override PoolBuilder<Func<Room>> ConfigureRoomPool(PoolBuilder<Func<Room>> pool) => pool
            .Include(() => new EmptyRoom(), 1)
            .Guarantee(() => new ShopRoom(), minAmount: 1)
            .Include(() => new TreasureRoom(), 1)
            .Include(() => new ShrineRoom(), 1);
        protected override PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> ConfigureEnemyPool(PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> pool) => pool
            .Include(args => args.Entities.NPC_Rat(), 100)
            .Include(args => args.Entities.NPC_RatArcher(), 7.5f)
            .Include(args => args.Entities.NPC_RatArsonist(), 1)
            //.Include(args => args.Entities.NPC_RatApothecary(), 1)
            .Include(args => args.Entities.NPC_RatKnight(), 17.5f)
            .Include(args => args.Entities.NPC_RatPugilist(), 1)
            .Include(args => args.Entities.NPC_RatWizard(), 5)
            .Include(args => args.Entities.NPC_RatThief(), 1)
            .Include(args => args.Entities.NPC_RatCheese(), 0.05f)
            ;

        protected override Dice GetMonsterDice(Room room, FloorGenerationContext ctx) =>
            new(3, room.GetRects().Count() / 2 + 1, Bias: -1);

    }
}
