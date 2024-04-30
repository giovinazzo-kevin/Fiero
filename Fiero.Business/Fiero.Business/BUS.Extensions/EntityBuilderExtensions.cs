using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using LightInject;
using Unconcern.Common;
namespace Fiero.Business
{
    public static class EntityBuilderExtensions
    {
        public static IEntityBuilder<T> LoadState<T>(this IEntityBuilder<T> builder, string resourceName, bool throwOnFail = false)
            where T : Entity
        {
            var scripts = builder.Entities.ServiceFactory.GetInstance<GameScripts<ScriptName>>();
            var entities = (ErgoScript)scripts.Get(ScriptName.Entity);
            var queryStr = $"dict({resourceName.ToErgoCase()}, X)";
            var query = entities.VM.ParseAndCompileQuery(queryStr);
            entities.VM.Query = query;
            entities.VM.Run();
            if (entities.VM.State == Ergo.Runtime.ErgoVM.VMState.Fail)
                if (throwOnFail)
                    throw new ArgumentException(resourceName);
                else
                    return builder;
            // X == {prop: prop_component{}, other_prop: other_prop_component{}, ...}
            var subs = entities.VM.Solutions.Single().Substitutions;
            var X = (Ergo.Lang.Ast.Set)(subs[new("X")].Substitute(subs));
            var kvps = X.Contents
                .ToDictionary(a => (Atom)((Complex)a).Arguments[0], a => ((Complex)a).Arguments[1]);
            foreach (var prop in builder.Entities.GetProxyableProperties<T>())
            {
                var name = new Atom(prop.Name.ToErgoCase());
                if (kvps.TryGetValue(name, out var dict))
                {
                    builder = builder.Load(prop.PropertyType, (Dict)dict);
                }
            }
            return builder;
        }
        public static IEntityBuilder<T> WithName<T>(this IEntityBuilder<T> builder, string name)
            where T : Entity => builder.AddOrTweak<InfoComponent>((s, c) => c.Name = s.GetInstance<GameResources>().Localizations.Translate(name));
        public static IEntityBuilder<T> WithDescription<T>(this IEntityBuilder<T> builder, string desc)
            where T : Entity => builder.AddOrTweak<InfoComponent>((s, c) => c.Description = s.GetInstance<GameResources>().Localizations.Translate(desc));
        public static IEntityBuilder<T> WithEffectTracking<T>(this IEntityBuilder<T> builder)
            where T : Entity => builder.AddOrTweak<EffectsComponent>();
        public static IEntityBuilder<T> WithIntrinsicEffect<T>(this IEntityBuilder<T> builder, EffectDef def, Func<EffectDef, Effect> wrap = null)
            where T : Entity => builder.AddOrTweak<EffectsComponent>((s, c) =>
            {
                c.Intrinsic.Add(def);
                builder.Built += OnBuilt;
                void OnBuilt(IEntityBuilder<T> b, T o)
                {
                    builder.Built -= OnBuilt;
                    var systems = s.GetInstance<MetaSystem>();
                    var sub = new Subscription(throwOnDoubleDispose: false);
                    wrap ??= (e => e.Resolve(o));
                    var fx = wrap(def);
                    fx.Start(systems, o, null);
                    c.Description += c.Describe(def, fx);
                    sub.Dispose();
                }
            });
        public static IEntityBuilder<T> WithPhysics<T>(this IEntityBuilder<T> builder, Coord pos, bool canMove = false, bool blocksMovement = false, bool blocksLight = false)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>((s, c) =>
            {
                c.Position = pos;
                c.BlocksLight = blocksMovement;
                c.BlocksLight = blocksLight;
                c.CanMove = canMove;
            });
        public static IEntityBuilder<T> WithMoveDelay<T>(this IEntityBuilder<T> builder, int moveDelay)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>((s, c) =>
            {
                c.MoveDelay = moveDelay;
            });
        public static IEntityBuilder<T> WithPosition<T>(this IEntityBuilder<T> builder, Coord pos, FloorId? floorId = null)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>((s, c) =>
            {
                c.Position = pos;
                if (floorId.HasValue)
                {
                    c.FloorId = floorId.Value;
                }
            });
        public static IEntityBuilder<T> WithSprite<T>(this IEntityBuilder<T> builder, RenderLayerName layer, TextureName texture, string sprite, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>((s, c) =>
            {
                c.Layer = layer;
                c.Texture = texture;
                c.Sprite = sprite;
                c.Color = color;
            });
        public static IEntityBuilder<Tile> WithVariant(this IEntityBuilder<Tile> builder, TileVariant variant)
            => builder.AddOrTweak<TileComponent>((s, c) =>
            {
                if (variant.Matrix.Middle != c.Name)
                    throw new ArgumentException(nameof(variant));
                c.Variants.Add(variant);
            });
        public static IEntityBuilder<T> WithItemSprite<T>(this IEntityBuilder<T> builder, string sprite)
            where T : Item => builder.AddOrTweak<ItemComponent>((s, c) =>
            {
                c.ItemSprite = sprite;
            });
        public static IEntityBuilder<T> WithTrailSprite<T>(this IEntityBuilder<T> builder, string sprite)
            where T : Projectile => builder.AddOrTweak<ProjectileComponent>((s, c) =>
            {
                c.TrailSprite = sprite;
            });
        public static IEntityBuilder<T> WithColor<T>(this IEntityBuilder<T> builder, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>((s, c) =>
            {
                c.Color = color;
            });
        public static IEntityBuilder<T> WithActorInfo<T>(this IEntityBuilder<T> builder, ActorName type)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static IEntityBuilder<T> WithRace<T>(this IEntityBuilder<T> builder, RaceName race)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Race = race;
            });
        public static IEntityBuilder<T> WithHealth<T>(this IEntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Health = new(0, maximum, current ?? maximum);
            });
        public static IEntityBuilder<T> WithMagic<T>(this IEntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Magic = new(0, maximum, current ?? maximum);
            });
        public static IEntityBuilder<T> WithExperience<T>(this IEntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Experience = new(0, maximum, current ?? maximum);
            });
        public static IEntityBuilder<T> WithLevel<T>(this IEntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Level = new(1, maximum, current ?? 1);
            });
        public static IEntityBuilder<T> WithCorpse<T>(this IEntityBuilder<T> builder, CorpseName type, Chance chance)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Corpse = new(type, chance);
            });
        public static IEntityBuilder<T> WithInventory<T>(this IEntityBuilder<T> builder, int capacity)
            where T : Actor => builder.AddOrTweak<InventoryComponent>((s, c) => c.Capacity = capacity);
        public static IEntityBuilder<T> WithItems<T>(this IEntityBuilder<T> builder, params Item[] items)
            where T : PhysicalEntity => builder.AddOrTweak<InventoryComponent>((s, c) =>
            {
                c.Capacity = Math.Max(c.Capacity, items.Length);
                // Delegate the actual adding of each item to when the owner of the inventory is spawned
                builder.Built += OnBuilt;
                void OnBuilt(IEntityBuilder<T> b, T e)
                {
                    builder.Built -= OnBuilt;
                    if (e is Actor)
                    {
                        var actionSystem = s.GetInstance<ActionSystem>();
                        actionSystem.ActorSpawned.SubscribeUntil(e =>
                        {
                            if (e.Actor.Id != c.EntityId)
                            {
                                return false;
                            }
                            foreach (var item in items)
                            {
                                actionSystem.ItemPickedUp.HandleOrThrow(new(e.Actor, item));
                            }
                            return true;
                        });
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            e.Inventory.TryPut(item, out _);
                        }
                    }
                }

            });
        public static IEntityBuilder<T> WithSpellLibrary<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>();
        public static IEntityBuilder<T> WithSpells<T>(this IEntityBuilder<T> builder, params Spell[] spells)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>((s, c) =>
            {
                // Delegate the actual adding of each spell to when the owner of the library is spawned
                builder.Built += OnBuilt;
                void OnBuilt(IEntityBuilder<T> b, T e)
                {
                    builder.Built -= OnBuilt;
                    var actionSystem = s.GetInstance<ActionSystem>();
                    actionSystem.ActorSpawned.SubscribeUntil(e =>
                    {
                        if (e.Actor.Id != c.EntityId)
                        {
                            return false;
                        }
                        foreach (var spell in spells)
                        {
                            actionSystem.SpellLearned.Handle(new(e.Actor, spell));
                        }
                        return true;
                    });
                }
            });
        public static IEntityBuilder<T> WithActorEquipment<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActorEquipmentComponent>();
        public static IEntityBuilder<T> WithIdleAi<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<IdleActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static IEntityBuilder<T> WithShopKeeperAi<T>(this IEntityBuilder<T> builder, Location home, Room room, string keeperTag)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                var provider = s.GetInstance<ShopKeeperActionProvider>();
                provider.Shop = new(home, room, keeperTag);
                c.ActionProvider = provider;

                builder.Built += OnBuilt;
                void OnBuilt(IEntityBuilder<T> b, T e)
                {
                    builder.Built -= OnBuilt;
                    provider.ShopKeeper = e;
                }
            })
                .AddOrTweak<AiComponent>();
        public static IEntityBuilder<T> WithEnemyAi<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<AiActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static IEntityBuilder<T> WithAutoPlayerAi<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<AutoPlayerActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static IEntityBuilder<T> WithLikedItems<T>(this IEntityBuilder<T> builder, params Func<Item, bool>[] likedItems)
            where T : Actor => builder.AddOrTweak<AiComponent>((s, c) =>
            {
                c.LikedItems.AddRange(likedItems);
            });
        public static IEntityBuilder<T> WithDislikedItems<T>(this IEntityBuilder<T> builder, params Func<Item, bool>[] dislikedItems)
            where T : Actor => builder.AddOrTweak<AiComponent>((s, c) =>
            {
                c.DislikedItems.AddRange(dislikedItems);
            });
        public static IEntityBuilder<T> WithPlayerAi<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<PlayerActionProvider>();
            });
        public static IEntityBuilder<T> WithFieldOfView<T>(this IEntityBuilder<T> builder, int radius)
            where T : Actor => builder.AddOrTweak<FieldOfViewComponent>((s, c) =>
            {
                c.Radius = radius;
            });
        public static IEntityBuilder<T> WithFaction<T>(this IEntityBuilder<T> builder, FactionName faction)
            where T : Actor => builder.AddOrTweak<FactionComponent>((s, c) =>
            {
                c.Name = faction;
            });
        public static IEntityBuilder<T> WithParty<T>(this IEntityBuilder<T> builder, Actor leader = null)
            where T : Actor => builder.AddOrTweak<PartyComponent>((s, c) =>
            {
                c.Leader = leader;
            });
        public static IEntityBuilder<T> WithLogging<T>(this IEntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<LogComponent>();
        public static IEntityBuilder<T> WithNpcInfo<T>(this IEntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<NpcComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static IEntityBuilder<T> WithPortalInfo<T>(this IEntityBuilder<T> builder, FloorConnection conn)
            where T : Feature => builder.AddOrTweak<PortalComponent>((s, c) =>
            {
                c.Connection = conn;
            });
        public static IEntityBuilder<T> WithFeatureInfo<T>(this IEntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<FeatureComponent>((s, c) =>
            {
                c.Name = type;
            });
        public static IEntityBuilder<T> WithTileInfo<T>(this IEntityBuilder<T> builder, TileName type)
            where T : Tile => builder.AddOrTweak<TileComponent>((s, c) =>
            {
                c.Name = type;
            });
        public static IEntityBuilder<T> WithConsumableInfo<T>(this IEntityBuilder<T> builder, int remainingUses, int maxUses, bool consumable, int chargesConsumedPerUse = 1)
            where T : Consumable => builder.AddOrTweak<ConsumableComponent>((s, c) =>
            {
                c.RemainingUses = remainingUses;
                c.MaximumUses = maxUses;
                c.ConsumedWhenEmpty = consumable;
                c.UsesConsumedPerAction = chargesConsumedPerUse;
            });
        public static IEntityBuilder<T> WithProjectileInfo<T>(this IEntityBuilder<T> builder, ProjectileName name, int damage, int maxRange, float mulchChance, bool throwsUseCharges, TrajectoryName @throw, bool piercing, bool directional)
            where T : Projectile => builder.AddOrTweak<ProjectileComponent>((s, c) =>
            {
                c.Name = name;
                c.BaseDamage = damage;
                c.MaximumRange = maxRange;
                c.MulchChance = mulchChance;
                c.ThrowsUseCharges = throwsUseCharges;
                c.Trajectory = @throw;
                c.Piercing = piercing;
                c.Directional = directional;
            });
        public static IEntityBuilder<T> WithLauncherInfo<T, TProj>(this IEntityBuilder<T> builder, IEntityBuilder<TProj> projBuilder)
            where T : Item
            where TProj : Projectile
            => builder.AddOrTweak<LauncherComponent>((s, c) =>
            {
                c.Projectile = projBuilder.Build();
            });
        public static IEntityBuilder<T> WithResourceInfo<T>(this IEntityBuilder<T> builder, ResourceName name, int amount, int maxAmount)
            where T : Resource => builder.AddOrTweak<ResourceComponent>((s, c) =>
            {
                c.Name = name;
                c.Amount = amount;
                c.MaximumAmount = maxAmount;
            });
        public static IEntityBuilder<T> WithPotionInfo<T>(this IEntityBuilder<T> builder, EffectDef quaffEffect, EffectDef throwEffect)
            where T : Potion => builder.AddOrTweak<PotionComponent>((s, c) =>
            {
                c.QuaffEffect = quaffEffect;
                c.ThrowEffect = throwEffect;
            });
        public static IEntityBuilder<T> WithFoodInfo<T>(this IEntityBuilder<T> builder, EffectDef eatEffect)
            where T : Food => builder.AddOrTweak<FoodComponent>((s, c) =>
            {
                c.EatEffect = eatEffect;
            });
        public static IEntityBuilder<T> WithWandInfo<T>(this IEntityBuilder<T> builder, EffectDef effect)
            where T : Wand => builder.AddOrTweak<WandComponent>((s, c) =>
            {
                c.Effect = effect;
            });
        public static IEntityBuilder<T> WithScrollInfo<T>(this IEntityBuilder<T> builder, EffectDef effect, ScrollModifierName modifier)
            where T : Scroll => builder.AddOrTweak<ScrollComponent>((s, c) =>
            {
                c.Effect = effect;
                c.Modifier = modifier;
            });
        public static IEntityBuilder<T> WithSpellInfo<T>(this IEntityBuilder<T> builder, TargetingShape shape, SpellName effect, int damage, int delay)
            where T : Spell => builder.AddOrTweak<SpellComponent>((s, c) =>
            {
                c.Name = effect;
                c.TargetingShape = shape;
                c.TargetingFilter = (_, __, ___) => true;
                c.BaseDamage = damage;
                c.CastDelay = delay;
            });
        public static IEntityBuilder<T> WithTargetingShape<T>(this IEntityBuilder<T> builder, TargetingShape shape)
            where T : Spell => builder.Tweak<SpellComponent>((s, c) =>
            {
                c.TargetingShape = shape;
            });
        public static IEntityBuilder<T> WithTargetingFilter<T>(this IEntityBuilder<T> builder, Func<MetaSystem, Actor, PhysicalEntity, bool> targetFilter)
            where T : Spell => builder.Tweak<SpellComponent>((s, c) =>
            {
                c.TargetingFilter = targetFilter;
            });

        public static IEntityBuilder<T> WithWeaponInfo<T>(this IEntityBuilder<T> builder, WeaponName type, Dice baseDamage, int swingDelay)
            where T : Weapon => builder.AddOrTweak<WeaponComponent>((s, c) =>
            {
                c.Type = type;
                c.BaseDamage = baseDamage;
                c.SwingDelay = swingDelay;
            });
        public static IEntityBuilder<T> WithCorpseInfo<T>(this IEntityBuilder<T> builder, CorpseName type)
            where T : Corpse => builder.AddOrTweak<CorpseComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static IEntityBuilder<T> WithItemInfo<T>(this IEntityBuilder<T> builder, int rarity, int goldValue, string unidentName = null)
            where T : Item => builder.AddOrTweak<ItemComponent>((s, c) =>
            {
                c.Rarity = rarity;
                c.BuyValue = goldValue;
                if (unidentName != null)
                    c.UnidentifiedName = s.GetInstance<GameResources>().Localizations.Translate(unidentName);
                c.Identified = String.IsNullOrEmpty(unidentName);
            });
        public static IEntityBuilder<T> WithShopTag<T>(this IEntityBuilder<T> builder, string ownerTag)
            where T : Item
        {
            builder = builder
                .AddOrTweak<ItemComponent>((s, c) => c.OwnerTag = ownerTag)
                .AddOrTweak<RenderComponent>((s, c) => c.BorderColor = ColorName.LightCyan);
            builder.Built += (e, f) => f.Render.Label = $"${f.GetBuyValue()}";
            return builder;
        }
        public static IEntityBuilder<T> WithEquipmentInfo<T>(this IEntityBuilder<T> builder, EquipmentTypeName type)
            where T : Equipment => builder.AddOrTweak<EquipmentComponent>((s, c) =>
            {
                c.Type = type;
            });
        //public static IEntityBuilder<T> WithScriptInfo<T>(this IEntityBuilder<T> builder, string fileName, bool trace = false, bool cache = false)
        //    where T : Script => builder.AddOrTweak<ErgoScriptComponent>((s, c) =>
        //    {
        //        c.ScriptPath = fileName;
        //        c.ShowTrace = trace;
        //        c.Cached = cache;
        //        // Delegate the actual loading of the script to when the entity is built
        //        builder.Built += OnBuilt;
        //        void OnBuilt(EntityBuilder<T> b, T e)
        //        {
        //            builder.Built -= OnBuilt;
        //            var scriptSystem = s.GetInstance<ScriptingSystem>();
        //            if (!scriptSystem.LoadScript(e))
        //            {
        //                // Flag script as invalid
        //                return;
        //            }
        //        }
        //    });
        public static IEntityBuilder<T> WithTraitTracking<T>(this IEntityBuilder<T> builder)
            where T : Entity => builder.AddOrTweak<TraitsComponent>();
        public static IEntityBuilder<T> WithIntrinsicTrait<T>(this IEntityBuilder<T> builder, Trait trait)
            where T : Entity => builder.AddOrTweak<TraitsComponent>((s, c) =>
            {
                c.AddIntrinsicTrait(trait);
            })
            .WithIntrinsicEffect(trait.Effect);
    }
}
