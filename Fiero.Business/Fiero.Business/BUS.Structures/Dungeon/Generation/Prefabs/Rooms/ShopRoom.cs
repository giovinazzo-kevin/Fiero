namespace Fiero.Business
{
    public class ShopRoom : Room
    {
        public ShopRoom()
        {
            AllowMonsters = false;
            AllowFeatures = false;
            AllowItems = false;
        }
        protected override DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            theme = base.CustomizeTheme(theme);
            return theme with
            {
                WallTile = (c => theme.WallTile(c).WithCustomColor(ColorName.LightYellow)),
                RoomTile = (c => theme.ShopTile(c).WithCustomColor(ColorName.Yellow))
            };
        }



        public override void Draw(FloorGenerationContext ctx)
        {
            base.Draw(ctx);
            var pos = Rects.Shuffle(Rng.Random).First().Center();
            var itemDice = new Dice(1, 6);
            var numItems = Enumerable.Range(0, Rects.Count)
                .SelectMany(_ => itemDice.Roll())
                .ToList();
            var possibleItemLocations = GetPointCloud()
                .Where(x => x.X % 2 == 0 && x.Y % 2 == 1 && ctx.GetTile(x).Name == TileName.Shop)
                .Shuffle(Rng.Random)
                .Take(numItems.Sum())
                .ToList();
            var itemPool = new PoolBuilder<Func<GameEntityBuilders, IEntityBuilder<Item>>>()
                .Include(e => e.Weapon_Sword().WithShopTag(), 1)
                .Include(e => e.Weapon_Hammer().WithShopTag(), 1)
                .Include(e => e.Weapon_Spear().WithShopTag(), 1)
                .Include(e => e.Weapon_Dagger().WithShopTag(), 1)
                .Include(e => e.Weapon_Bow().WithShopTag(), 1)
                .Include(e => e.Weapon_Crossbow().WithShopTag(), 1)
                .Include(e => e.Scroll_OfMagicMapping().WithShopTag(), 1)
                .Include(e => e.Scroll_OfMassConfusion().WithShopTag(), 1)
                .Include(e => e.Scroll_OfMassEntrapment().WithShopTag(), 1)
                .Include(e => e.Scroll_OfMassSleep().WithShopTag(), 1)
                .Include(e => e.Scroll_OfMassSilence().WithShopTag(), 1)
                .Include(e => e.Wand_OfConfusion().WithShopTag(), 1)
                .Include(e => e.Wand_OfEntrapment().WithShopTag(), 1)
                .Include(e => e.Wand_OfPoison().WithShopTag(), 1)
                .Include(e => e.Wand_OfSilence().WithShopTag(), 1)
                .Include(e => e.Wand_OfSleep().WithShopTag(), 1)
                .Include(e => e.Wand_OfTeleport().WithShopTag(), 1)
                .Include(e => e.Potion_OfConfusion().WithShopTag(), 1)
                .Include(e => e.Potion_OfEntrapment().WithShopTag(), 1)
                .Include(e => e.Potion_OfHealing().WithShopTag(), 10)
                .Include(e => e.Potion_OfSilence().WithShopTag(), 1)
                .Include(e => e.Potion_OfSleep().WithShopTag(), 1)
                .Include(e => e.Potion_OfTeleport().WithShopTag(), 1)
                .Include(e => e.Projectile_Rock(charges: Rng.Random.Next(3, 10)).WithShopTag(), 2)
                .Include(e => e.Projectile_Arrow(charges: Rng.Random.Next(3, 10)).WithShopTag(), 2)
                .Include(e => e.Projectile_Bomb(charges: Rng.Random.Next(3, 10)).WithShopTag(), 2)
                .Build(32);
            var k = 0;
            for (int i = 0; i < numItems.Count; i++)
                for (int j = 0; j < numItems[i]; j++)
                {
                    var nextLocation = possibleItemLocations[k++];
                    ctx.AddObject("sold_item", nextLocation, x => itemPool.Next()(x));
                }
            ctx.AddObject("shopkeeper", pos, e => e.NPC_RatMerchant()
                .WithShopKeeperAi(new(ctx.Id, pos), this)
                .WithFaction(FactionName.Merchants)
                .LoadState("merchant"));
        }
    }
}
