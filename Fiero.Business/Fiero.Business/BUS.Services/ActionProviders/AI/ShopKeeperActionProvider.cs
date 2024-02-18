namespace Fiero.Business
{
    [TransientDependency]
    public class ShopKeeperActionProvider : AiActionProvider
    {
        public readonly record struct ShopDef(Location Home, Room Room, string OwnerTag);
        public readonly record struct DebtDef(int PlayerId, int AmountOwed);
        public ShopDef Shop { get; set; }
        public Actor ShopKeeper { get; set; }
        private readonly List<Actor> playersInShop = new();
        private readonly List<Item> itemsBeingSold = new();
        private readonly List<Actor> playersBeingChased = new();
        private readonly Dictionary<int, DebtDef> debtTable = new();


        private bool IsInShopArea(Coord c)
            => Shop.Room.GetRects().Any(r => r.Contains(c.X, c.Y))
            && Systems.Get<DungeonSystem>().GetTileAt(Shop.Home.FloorId, c) is { TileProperties.Name: TileName.Shop };

        public ShopKeeperActionProvider(MetaSystem sys) : base(sys)
        {
            var entityBuilders = sys.Resolve<GameEntityBuilders>();
            var action = sys.Get<ActionSystem>();
            var currentGen = action.CurrentGeneration;
            action.ItemPickedUp.SubscribeUntil(e =>
            {
                if (ShopKeeper == null || ShopKeeper.IsInvalid()
                    || action.CurrentGeneration != currentGen)
                    return true;
                if (playersInShop.Contains(e.Actor) && e.Item.ItemProperties.OwnerTag == Shop.OwnerTag)
                {
                    StoreDebt(e.Actor, e.Item, pickedUp: true);
                }
                return false;
            });
            action.ItemDropped.SubscribeUntil(e =>
            {
                if (ShopKeeper == null || ShopKeeper.IsInvalid()
                    || action.CurrentGeneration != currentGen)
                    return true;
                if (playersInShop.Contains(e.Actor))
                {
                    StoreDebt(e.Actor, e.Item, pickedUp: false);
                }
                return false;
            });
            // Keep track of players entering and leaving the shop
            action.ActorMoved.SubscribeUntil(e =>
            {
                if (ShopKeeper == null || ShopKeeper.IsInvalid()
                    || action.CurrentGeneration != currentGen)
                    return true;
                if (!playersInShop.Contains(e.Actor)
                && e.Actor.IsPlayer() && IsInShopArea(e.Actor.Position()))
                {
                    OnPlayerEnteredShop(e.Actor);
                }
                else if (playersInShop.Contains(e.Actor)
                && e.Actor.IsPlayer() && !IsInShopArea(e.Actor.Position()))
                {
                    OnPlayerLeftShop(e.Actor);
                }
                return false;
            });
            // Keep track of players entering and leaving the shop
            Systems.Get<DialogueSystem>().DialogueTriggered.SubscribeUntil(e =>
            {
                if (ShopKeeper == null || ShopKeeper.IsInvalid()
                    || action.CurrentGeneration != currentGen)
                    return true;
                if (e.Listeners.OfType<Actor>().FirstOrDefault(x => x.IsPlayer()) is not Actor player
                || !debtTable.TryGetValue(player.Id, out var debt))
                    return false;
                switch (e.Node.Id)
                {
                    case "Merchant_Transact":
                        var playerGold = player.Inventory.GetResources()
                            .SingleOrDefault(r => r.ResourceProperties.Name == ResourceName.Gold)
                            ?? entityBuilders.Resource_Gold(amount: 0)
                                .Build();
                        // shopkeeper pays player
                        if (debt.AmountOwed < 0)
                        {
                            playerGold.ResourceProperties.Amount -= debt.AmountOwed;
                            // If the player didn't have any gold
                            if (!player.Inventory.GetResources().Contains(playerGold))
                            {
                                // Try to put it in their inventory or drop it on the ground
                                if (!player.Inventory.TryPut(playerGold, out _))
                                {
                                    playerGold.Physics.Position = player.Position();
                                    Systems.Get<DungeonSystem>().AddItem(Shop.Home.FloorId, playerGold);
                                }
                            }
                            debtTable.Remove(player.Id);
                            UntagItems(player);
                        }
                        // player pays shopkeeper
                        else
                        {
                            if (playerGold.ResourceProperties.Amount < debt.AmountOwed)
                            {
                                e.Node.ForceClose();
                                ShopKeeper.Dialogue.Triggers.Add(new ManualDialogueTrigger(Systems, "Merchant_CantAfford")
                                { Arguments = [debt.AmountOwed - playerGold.ResourceProperties.Amount] });
                                Systems.Get<DialogueSystem>().CheckTriggers();
                            }
                            else
                            {
                                playerGold.ResourceProperties.Amount -= debt.AmountOwed;
                                if (playerGold.ResourceProperties.Amount == 0)
                                    player.Inventory.TryTake(playerGold);
                                UntagItems(player);
                            }
                        }
                        break;
                    case "Merchant_Thief":
                        // now you've done it
                        Systems.Get<FactionSystem>().SetBilateralRelation(ShopKeeper, player, StandingName.Hated);
                        playersBeingChased.Add(player);
                        break;
                }
                return false;
            });
            // Killing the shopkeeper grants the player ownership over the shop items
            action.ActorDied.SubscribeUntil(e =>
            {
                if (action.CurrentGeneration != currentGen)
                    return true;
                if (e.Actor != ShopKeeper)
                    return false;
                foreach (var player in playersInShop)
                    UntagItems(player);
                playersInShop.Clear();
                // Also untag any shop items lying on the ground
                var shopItems = Systems.Get<DungeonSystem>()
                    .GetAllItems(Shop.Home.FloorId)
                    .Where(i => i.ItemProperties.OwnerTag == Shop.OwnerTag);
                foreach (var item in shopItems)
                {
                    item.ItemProperties.OwnerTag = null;
                    ClearLabel(item);
                }
                return true;
            });
        }

        protected void UntagItems(Actor player)
        {
            foreach (var item in player.Inventory.GetItems())
            {
                if (item.ItemProperties.OwnerTag == Shop.OwnerTag)
                {
                    item.ItemProperties.OwnerTag = null;
                    ClearLabel(item);
                }
            }
            foreach (var item in itemsBeingSold)
            {
                item.ItemProperties.OwnerTag = Shop.OwnerTag;
                SetBuyLabel(item);
            }
            itemsBeingSold.Clear();
        }

        protected static void SetSellLabel(Item item)
        {
            item.Render.Label = $"${item.GetSellValue()}";
            item.Render.BorderColor = ColorName.LightMagenta;
        }
        protected static void SetBuyLabel(Item item)
        {
            item.Render.Label = $"${item.GetBuyValue()}";
            item.Render.BorderColor = ColorName.LightCyan;
        }

        protected static void ClearLabel(Item item)
        {
            item.Render.Label = null;
            item.Render.BorderColor = null;
        }

        protected void StoreDebt(Actor player, Item item, bool pickedUp)
        {
            if (!debtTable.TryGetValue(player.Id, out var debt))
                debt = new(player.Id, 0);
            var isFromShop = item.ItemProperties.OwnerTag == Shop.OwnerTag;
            var value = isFromShop
                ? item.GetBuyValue()
                : -item.GetSellValue();
            if (!pickedUp && isFromShop || pickedUp && !isFromShop)
                value = -value;
            if (pickedUp && !isFromShop)
            {
                ClearLabel(item);
                itemsBeingSold.Remove(item);
            }
            else if (!pickedUp && !isFromShop)
            {
                SetSellLabel(item);
                itemsBeingSold.Add(item);
            }
            debtTable[player.Id] = debt with { AmountOwed = debt.AmountOwed + value };
        }

        protected void OnPlayerLeftShop(Actor player)
        {
            playersInShop.Remove(player);
            if (!debtTable.TryGetValue(player.Id, out var debt) || debt.AmountOwed == 0)
                return; // player is just passing
            if (playersBeingChased.Contains(player))
                return; // shopkeeper hates the player
            var nodeChoice = debt.AmountOwed > 0 ? "Merchant_YouOweMe" : "Merchant_IOweYou";
            ShopKeeper.Dialogue.Triggers.Add(new ManualDialogueTrigger(Systems, nodeChoice)
            {
                Arguments = [Math.Abs(debt.AmountOwed)]
            });
        }

        protected void OnPlayerEnteredShop(Actor player)
        {
            playersInShop.Add(player);
        }

        protected override IAction Wander(Actor a)
        {
            var floor = Systems.Get<DungeonSystem>();
            if (playersBeingChased.Count > 0)
                TryPushObjective(a, playersBeingChased.Last());
            else if (playersInShop.Count > 0)
                TryPushObjective(a, Rng.Random.Choose(playersInShop));
            else
                TryPushObjective(a, floor.GetTileAt(Shop.Home.FloorId, Shop.Home.Position));
            if (TryFollowPath(a, out var action))
            {
                return action;
            }
            return new WaitAction();
        }

        public override IAction GetIntent(Actor a)
        {
            if (!ShopKeeper.IsInvalid() && ShopKeeper != a)
                throw new InvalidOperationException();
            return base.GetIntent(a);
        }
    }
}
