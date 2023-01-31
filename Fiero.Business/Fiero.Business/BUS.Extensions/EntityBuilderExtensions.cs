using Fiero.Core;
using LightInject;
using System;

namespace Fiero.Business
{
    public static class EntityBuilderExtensions
    {
        public static EntityBuilder<T> WithName<T>(this EntityBuilder<T> builder, string name)
            where T : Entity => builder.AddOrTweak<InfoComponent>(c => c.Name = name);
        public static EntityBuilder<T> WithEffectTracking<T>(this EntityBuilder<T> builder)
            where T : Entity => builder.AddOrTweak<EffectsComponent>();
        public static EntityBuilder<T> WithIntrinsicEffect<T>(this EntityBuilder<T> builder, EffectDef def, Func<EffectDef, Effect> wrap = null)
            where T : Entity => builder.AddOrTweak<EffectsComponent>(c =>
            {
                c.Intrinsic.Add(def);
                builder.Built += (b, o) =>
                {
                    wrap ??= (e => e.Resolve(o));
                    var fx = wrap(def);
                    if (fx is ScriptEffect se && se.Script.ScriptProperties.LastError != null)
                    {
                        // If a script errored out during initialization, for instance by failing to load,
                        // then don't start it. Error messages are handled by the WithScriptInfo method.
                        return;
                    }
                    fx.Start(b.ServiceFactory.GetInstance<GameSystems>(), o);
                };
            });
        public static EntityBuilder<T> WithPhysics<T>(this EntityBuilder<T> builder, Coord pos, bool canMove = false, bool blocksMovement = false, bool blocksLight = false)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>(c =>
            {
                c.Position = pos;
                c.BlocksLight = blocksMovement;
                c.BlocksLight = blocksLight;
                c.CanMove = canMove;
            });
        public static EntityBuilder<T> WithPosition<T>(this EntityBuilder<T> builder, Coord pos, FloorId? floorId = null)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>(c =>
            {
                c.Position = pos;
                if (floorId.HasValue)
                {
                    c.FloorId = floorId.Value;
                }
            });
        public static EntityBuilder<T> WithSprite<T>(this EntityBuilder<T> builder, RenderLayerName layer, TextureName texture, string sprite, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>(c =>
            {
                c.Layer = layer;
                c.Texture = texture;
                c.Sprite = sprite;
                c.Color = color;
            });
        public static EntityBuilder<T> WithColor<T>(this EntityBuilder<T> builder, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>(c =>
            {
                c.Color = color;
            });
        public static EntityBuilder<T> WithActorInfo<T>(this EntityBuilder<T> builder, ActorName type)
            where T : Actor => builder.AddOrTweak<ActorComponent>(c =>
            {
                c.Type = type;
            });
        public static EntityBuilder<T> WithHealth<T>(this EntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>(c =>
            {
                c.Health = new(0, maximum, current ?? maximum);
            });
        public static EntityBuilder<T> WithInventory<T>(this EntityBuilder<T> builder, int capacity)
            where T : Actor => builder.AddOrTweak<InventoryComponent>(c => c.Capacity = capacity);
        public static EntityBuilder<T> WithItems<T>(this EntityBuilder<T> builder, params Item[] items)
            where T : PhysicalEntity => builder.AddOrTweak<InventoryComponent>(c =>
            {
                c.Capacity = Math.Max(c.Capacity, items.Length);
                // Delegate the actual adding of each item to when the owner of the inventory is spawned
                builder.Built += (b, e) =>
                {
                    var actionSystem = b.ServiceFactory.GetInstance<ActionSystem>();
                    actionSystem.ActorSpawned.SubscribeUntil(e =>
                    {
                        if (e.Actor.Id != c.EntityId)
                        {
                            return false;
                        }
                        foreach (var item in items)
                        {
                            actionSystem.ItemPickedUp.Raise(new(e.Actor, item));
                        }
                        return true;
                    });
                };
            });
        public static EntityBuilder<T> WithSpellLibrary<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>();
        public static EntityBuilder<T> WithSpells<T>(this EntityBuilder<T> builder, params Spell[] spells)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>(c =>
            {
                // Delegate the actual adding of each spell to when the owner of the library is spawned
                builder.Built += (b, e) =>
                {
                    var actionSystem = b.ServiceFactory.GetInstance<ActionSystem>();
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
                };
            });
        public static EntityBuilder<T> WithEquipment<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<EquipmentComponent>();
        public static EntityBuilder<T> WithEnemyAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>(c =>
            {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                c.ActionProvider = new AiActionProvider(gameSystems);
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithAutoPlayerAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>(c =>
            {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                c.ActionProvider = new AutoPlayerActionProvider(gameSystems);
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithLikedItems<T>(this EntityBuilder<T> builder, params Func<Item, bool>[] likedItems)
            where T : Actor => builder.AddOrTweak<AiComponent>(c =>
            {
                c.LikedItems.AddRange(likedItems);
            });
        public static EntityBuilder<T> WithPlayerAi<T>(this EntityBuilder<T> builder, GameUI ui)
            where T : Actor => builder.AddOrTweak<ActionComponent>(c =>
            {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                var quickSlots = (QuickSlotHelper)builder.ServiceFactory.GetInstance(typeof(QuickSlotHelper));
                c.ActionProvider = new PlayerActionProvider(ui, gameSystems, quickSlots);
            });
        public static EntityBuilder<T> WithFieldOfView<T>(this EntityBuilder<T> builder, int radius)
            where T : Actor => builder.AddOrTweak<FieldOfViewComponent>(c =>
            {
                c.Radius = radius;
            });
        public static EntityBuilder<T> WithFaction<T>(this EntityBuilder<T> builder, FactionName faction)
            where T : Actor => builder.AddOrTweak<FactionComponent>(c =>
            {
                c.Name = faction;
            });
        public static EntityBuilder<T> WithLogging<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<LogComponent>(_ => { });
        public static EntityBuilder<T> WithNpcInfo<T>(this EntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<NpcComponent>(c => c.Type = type);
        public static EntityBuilder<T> WithDialogueTriggers<T>(this EntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<DialogueComponent>(c =>
            {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                gameSystems.Dialogue.SetTriggers(gameSystems, type, c);
            });
        public static EntityBuilder<T> WithDialogueTriggers<T>(this EntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<DialogueComponent>(c =>
            {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                gameSystems.Dialogue.SetTriggers(gameSystems, type, c);
            });
        public static EntityBuilder<T> WithPortalInfo<T>(this EntityBuilder<T> builder, FloorConnection conn)
            where T : Feature => builder.AddOrTweak<PortalComponent>(c =>
            {
                c.Connection = conn;
            });
        public static EntityBuilder<T> WithFeatureInfo<T>(this EntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<FeatureComponent>(c =>
            {
                c.Name = type;
            });
        public static EntityBuilder<T> WithTileInfo<T>(this EntityBuilder<T> builder, TileName type)
            where T : Tile => builder.AddOrTweak<TileComponent>(c =>
            {
                c.Name = type;
            });
        public static EntityBuilder<T> WithConsumableInfo<T>(this EntityBuilder<T> builder, int remainingUses, int maxUses, bool consumable)
            where T : Consumable => builder.AddOrTweak<ConsumableComponent>(c =>
            {
                c.RemainingUses = remainingUses;
                c.MaximumUses = maxUses;
                c.ConsumedWhenEmpty = consumable;
            });
        public static EntityBuilder<T> WithThrowableInfo<T>(this EntityBuilder<T> builder, ThrowableName name, int damage, int maxRange, float mulchChance, bool throwsUseCharges, ThrowName @throw)
            where T : Throwable => builder.AddOrTweak<ThrowableComponent>(c =>
            {
                c.Name = name;
                c.BaseDamage = damage;
                c.MaximumRange = maxRange;
                c.MulchChance = mulchChance;
                c.ThrowsUseCharges = throwsUseCharges;
                c.Throw = @throw;
            });
        public static EntityBuilder<T> WithResourceInfo<T>(this EntityBuilder<T> builder, ResourceName name, int amount, int maxAmount)
            where T : Resource => builder.AddOrTweak<ResourceComponent>(c =>
            {
                c.Name = name;
                c.Amount = amount;
                c.MaximumAmount = maxAmount;
            });
        public static EntityBuilder<T> WithPotionInfo<T>(this EntityBuilder<T> builder, EffectDef quaffEffect, EffectDef throwEffect)
            where T : Potion => builder.AddOrTweak<PotionComponent>(c =>
            {
                c.QuaffEffect = quaffEffect;
                c.ThrowEffect = throwEffect;
            });
        public static EntityBuilder<T> WithFoodInfo<T>(this EntityBuilder<T> builder, EffectDef eatEffect)
            where T : Food => builder.AddOrTweak<FoodComponent>(c =>
            {
                c.EatEffect = eatEffect;
            });
        public static EntityBuilder<T> WithWandInfo<T>(this EntityBuilder<T> builder, EffectDef effect)
            where T : Wand => builder.AddOrTweak<WandComponent>(c =>
            {
                c.Effect = effect;
            });
        public static EntityBuilder<T> WithScrollInfo<T>(this EntityBuilder<T> builder, EffectDef effect, ScrollModifierName modifier)
            where T : Scroll => builder.AddOrTweak<ScrollComponent>(c =>
            {
                c.Effect = effect;
                c.Modifier = modifier;
            });
        public static EntityBuilder<T> WithSpellInfo<T>(this EntityBuilder<T> builder, TargetingShape shape, SpellName effect, int damage, int delay)
            where T : Spell => builder.AddOrTweak<SpellComponent>(c =>
            {
                c.Name = effect;
                c.TargetingShape = shape;
                c.TargetingFilter = (_, __, ___) => true;
                c.BaseDamage = damage;
                c.CastDelay = delay;
            });
        public static EntityBuilder<T> WithTargetingShape<T>(this EntityBuilder<T> builder, TargetingShape shape)
            where T : Spell => builder.Tweak<SpellComponent>(c =>
            {
                c.TargetingShape = shape;
            });
        public static EntityBuilder<T> WithTargetingFilter<T>(this EntityBuilder<T> builder, Func<GameSystems, Actor, PhysicalEntity, bool> targetFilter)
            where T : Spell => builder.Tweak<SpellComponent>(c =>
            {
                c.TargetingFilter = targetFilter;
            });

        public static EntityBuilder<T> WithWeaponInfo<T>(this EntityBuilder<T> builder, WeaponName type, int baseDamage, int swingDelay)
            where T : Weapon => builder.AddOrTweak<WeaponComponent>(c =>
            {
                c.Type = type;
                c.BaseDamage = baseDamage;
                c.SwingDelay = swingDelay;
            });
        public static EntityBuilder<T> WithArmorInfo<T>(this EntityBuilder<T> builder, ArmorName type)
            where T : Armor => builder.AddOrTweak<ArmorComponent>(c =>
            {
                c.Type = type;
            });
        public static EntityBuilder<T> WithItemInfo<T>(this EntityBuilder<T> builder, int rarity, string unidentName = null)
            where T : Item => builder.AddOrTweak<ItemComponent>(c =>
            {
                c.Rarity = rarity;
                c.UnidentifiedName = unidentName;
                c.Identified = String.IsNullOrEmpty(unidentName);
            });
        public static EntityBuilder<T> WithScriptInfo<T>(this EntityBuilder<T> builder, string fileName)
            where T : Script => builder.AddOrTweak<ErgoScriptComponent>(c =>
            {
                c.ScriptPath = fileName;
                // Delegate the actual loading of the script to when the entity is built
                builder.Built += (b, e) =>
                {
                    var scriptSystem = b.ServiceFactory.GetInstance<ErgoScriptingSystem>();
                    if (!scriptSystem.LoadScript(e))
                    {
                        // TODO: Log to the in-game console that doesn't exist yet
                    }
                };
            });
    }
}
