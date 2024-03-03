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
            RoomSquares = new(2, 2, (d, s) => 1f / Math.Pow(s, 2)),
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
        protected override PoolBuilder<Func<Room>> ConfigureRoomPool(FloorId id, PoolBuilder<Func<Room>> pool) => pool
            .Guarantee(() => new EmptyRoom(), minAmount: 1)
            .Include(() => new WetFloorSewerRoom(), 1)
            .If(() => id.Depth > 1, pool => pool
                .Guarantee(() => new ShopRoom(), minAmount: 1, maxAmount: 1)
                .Include(() => new TreasureRoom(), 1, maxAmount: 1)
                .Include(() => new ShrineRoom(), 1, maxAmount: 1))
            ;
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
        protected override PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> ConfigureItemPool(PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> pool) => pool
            .Include(args => args.Entities.Projectile_Bomb(), 2.5f)
            .Include(args => args.Entities.Projectile_Arrow(), 5)
            .Include(args => args.Entities.Projectile_Grapple(), 0.1f)
            .Include(args => args.Entities.Projectile_Rock(), 20)
            .Include(args => args.Entities.Resource_Gold(amount: Rng.Random.Between(1, 100)), 10)
            .Include(args => args.Entities.Potion_OfConfusion(), 3)
            .Include(args => args.Entities.Potion_OfHealing(), 3)
            .Include(args => args.Entities.Potion_OfSleep(), 3)
            .Include(args => args.Entities.Wand_OfEntrapment(), 3)
            .Include(args => args.Entities.Wand_OfPoison(), 3)
            .Include(args => args.Entities.Wand_OfTeleport(), 3)
            .Include(args => args.Entities.Weapon_Hammer(), 1)
            .Include(args => args.Entities.Weapon_Sword(), 1)
            .Include(args => args.Entities.Weapon_Dagger(), 1)
            .Include(args => args.Entities.Weapon_Spear(), 1)
            .Include(args => args.Entities.Weapon_Bow(), 1)
            .Include(args => args.Entities.Weapon_Crossbow(), 1)
            ;

        protected override Dice GetMonsterDice(Room room, FloorGenerationContext ctx) =>
            new(2, 2, Bias: -1);

    }
}
