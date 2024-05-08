namespace Fiero.Business
{
    [TransientDependency]
    public class TestBranchGenerator : RoomTreeGenerator
    {
        public static readonly DungeonTheme DefaultTheme = DungeonTheme.Default with
        {
            RoomSquares = new(1, 1),
        };
        public readonly GameScripts<ScriptName> Scripts;
        public TestBranchGenerator(GameScripts<ScriptName> scripts) : base(DefaultTheme) { Scripts = scripts; }
        public override Coord MapSize(FloorId id) => id.Depth switch
        {
            _ => new(50, 50)
        };
        public override Coord GridSize(FloorId id) => id.Depth switch
        {
            _ => new(0, 0),
        };
        protected override PoolBuilder<Func<Room>> ConfigureRoomPool(FloorId id, PoolBuilder<Func<Room>> pool) => pool
            .Guarantee(() => new EmptyRoom())
            ;
        protected override PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> ConfigureEnemyPool(FloorId id, PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> pool) => pool
            .Include(args => args.Entities.NPC_Rat(), 100)
            ;
        protected override PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> ConfigureItemPool(FloorId id, PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> pool) => pool
            .If(() => id.Depth == 1, pool => pool
                .Guarantee(args => args.Entities.Weapon_Sword(), minAmount: 1, maxAmount: 1)
                .Guarantee(args => args.Entities.Weapon_Dagger(), minAmount: 1, maxAmount: 1)
                .Guarantee(args => args.Entities.Weapon_Hammer(), minAmount: 1, maxAmount: 1)
                .Guarantee(args => args.Entities.Weapon_Spear(), minAmount: 1, maxAmount: 1))
            .Include(args => args.Entities.Potion_OfHealing(), 2)
            .Include(args => args.Entities.Potion_OfConfusion(), 2)
            .Include(args => args.Entities.Projectile_Rock(), 100)
            ;

        protected override Dice GetItemDice(Room room, FloorGenerationContext ctx) =>
            new(5, 10, Bias: -9);

        protected override Dice GetMonsterDice(Room room, FloorGenerationContext ctx) =>
            new(2, 10, Bias: -0);
    }
}
