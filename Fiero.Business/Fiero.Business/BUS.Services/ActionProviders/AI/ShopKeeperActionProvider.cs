namespace Fiero.Business
{
    [TransientDependency]
    public class ShopKeeperActionProvider(MetaSystem sys) : AiActionProvider(sys)
    {
        public readonly record struct ShopDef(Location Home, Room Room);

        public ShopDef Shop { get; set; }
        private Actor trackedPlayer;

        void TrackPlayer(Actor shopKeeper, Actor player)
        {
            if (trackedPlayer != null)
                return;
            trackedPlayer = player;
            Systems.Get<ActionSystem>().ItemPickedUp.SubscribeUntil(e =>
            {
                if (e.Actor != player)
                    return false;
                if (e.Item.Render.BorderColor == ColorName.LightMagenta)
                {
                    e.Item.Render.BorderColor = null;
                    e.Item.Render.Label = null;
                }
                else if (e.Item.ItemProperties.IsFromShop &&
                    Chance.OneIn(10)
                    && Systems.Resolve<GameResources>().GetSpeechBubble(shopKeeper, SpeechName.Merchant_ItemPickedUp, out var speech))
                {
                    Systems.Get<RenderSystem>().AnimateViewport(blocking: false, shopKeeper, speech.Animation);
                }
                return trackedPlayer == null;
            });
            Systems.Get<ActionSystem>().ItemDropped.SubscribeUntil(e =>
            {
                if (e.Actor != player)
                    return false;
                if (!e.Item.ItemProperties.IsFromShop)
                {
                    e.Item.Render.BorderColor = ColorName.LightMagenta;
                    e.Item.Render.Label = $"${e.Item.ItemProperties.SellValue}";
                }
                return trackedPlayer == null;
            });
        }

        bool PaymentSequence(Actor shopKeeper, Actor player)
        {
            var ui = Systems.Resolve<GameUI>();
            var floor = Systems.Get<DungeonSystem>();

            var pos = shopKeeper.Position();
            var pPos = player.Position();
            // The player is trying to leave the shop
            if (!Shop.Room.GetRects().Any(r => r.Contains(pPos.X, pPos.Y)))
            {
                var itemsTaken = player.Inventory.GetItems()
                    .Where(i => i.ItemProperties.IsFromShop)
                    .ToHashSet();
                var itemsLeft = Shop.Room.GetPointCloud()
                    .SelectMany(p => floor.GetItemsAt(Shop.Home.FloorId, p))
                    .Where(i => !i.ItemProperties.IsFromShop)
                    .ToHashSet();
                if (itemsTaken.Count == 0 && itemsLeft.Count == 0)
                    return true; // player is just passing 
                if (shopKeeper.DistanceFrom(pPos) > 3)
                {
                    TpNextToPlayer(); // not so fast!
                }
                var costBuy = itemsTaken.Sum(x => x.ItemProperties.BuyValue);
                var costSell = itemsLeft.Sum(x => x.ItemProperties.SellValue);
                var owedGold = costBuy - costSell;
                var whichDialogue = owedGold switch
                {
                    > 0 => "",
                    < 0 => "",
                    0 => ""
                };

                var sellMsg = costSell > 0 ? $"\nFor your items, I can give you ${costSell}." : "";
                var buyMsg = costBuy > 0 ? $"\nThe items you bought amount to ${costBuy}." : "";
                var totalMsg = costSell > 0 && costBuy > 0 ?
                    owedGold > 0
                        ? $"\nSo... You owe me ${owedGold}, thank you."
                        : $"\nSo... I owe you ${-owedGold}, here."
                    : "";
                var message = $"Let's see.{sellMsg}{buyMsg}{totalMsg}";
                var opt_pay = owedGold > 0 ? "<Buy>" : "<Accept>";
                var opt_dontpay = "<Steal>";
                var choices = new List<string>() { opt_pay };
                if (owedGold > 0)
                    choices.Add(opt_dontpay);
                var modal = ui.NecessaryChoice(choices.ToArray(), message, "Shop");
                modal.OptionChosen += (e, option) =>
                {
                    var ok = true;
                    var angry = option.Equals(opt_dontpay);
                    if (!angry)
                    {
                        if (owedGold > 0)
                        {
                            var playerGold = player.Inventory.GetResources()
                                .Where(x => x.ResourceProperties.Name == ResourceName.Gold)
                                .SingleOrDefault();
                            var goldLeft = (playerGold?.ResourceProperties.Amount ?? 0) - owedGold;
                            if (goldLeft < 0)
                            {
                                shopKeeper.Dialogue.Triggers.Add(new ManualDialogueTrigger(Systems, "Merchant_CantAfford")
                                {
                                    Arguments = [(-goldLeft).ToString()]
                                });
                                ok = false;
                            }
                        }
                    }
                    if (angry)
                    {
                        Systems.Get<FactionSystem>()
                            .SetBilateralRelation(shopKeeper, player, StandingName.Hated);
                    }
                    if (ok)
                    {
                        foreach (var item in itemsTaken)
                        {
                            item.Render.BorderColor = null;
                            item.Render.Label = null;
                            item.ItemProperties.IsFromShop = false;
                        }
                        foreach (var item in itemsLeft)
                        {
                            item.Render.BorderColor = ColorName.LightCyan;
                            item.Render.Label = $"${item.ItemProperties.BuyValue}";
                            item.ItemProperties.IsFromShop = true;
                        }
                    }
                };
                return true;
            }
            return false;

            void TpNextToPlayer()
            {
                var nextToPlayer = Shapes.Neighborhood(pPos, 5)
                    .Shuffle(Rng.Random)
                    .Where(x => !floor.GetActorsAt(Shop.Home.FloorId, x).Any()
                            && (floor.GetTileAt(Shop.Home.FloorId, x)?.IsWalkable(shopKeeper) ?? false))
                    .OrderBy(x => x.DistSq(pPos))
                    .ToList();
                if (nextToPlayer.Count != 0)
                {
                    Systems.Get<ActionSystem>().ActorTeleporting.HandleOrThrow(new(shopKeeper, pos, nextToPlayer[0]));
                }
            }
        }

        protected override IAction Wander(Actor a)
        {
            var pos = a.Position();
            if (trackedPlayer != null)
            {
                if (PaymentSequence(a, trackedPlayer))
                    trackedPlayer = null;
            }

            if (Shop.Room.GetRects().Any(r => r.Contains(pos.X, pos.Y)))
            {
                if (NearbyAllies.Values
                    .Where(x => x.IsPlayer())
                    .FirstOrDefault() is { } player)
                {
                    // At this point, the shopkeeper has noticed the player and will follow them around.
                    if (a.DistanceFrom(player) > 2 && a.CanSee(player))
                        TryPushObjective(a, player);
                    TrackPlayer(a, player);
                }
            }
            else
            {
                TryPushObjective(a, Systems.Get<DungeonSystem>().GetTileAt(Shop.Home.FloorId, Shop.Home.Position));
            }
            if (TryFollowPath(a, out var action))
            {
                return action;
            }
            if (a.Ai.Target == null && RepathChance.Check(Rng.Random))
            {
                Repath(a);
                if (TryFollowPath(a, out action))
                {
                    return action;
                }
            }
            return new WaitAction();
        }
    }
}
