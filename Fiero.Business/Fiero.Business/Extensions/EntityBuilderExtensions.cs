using Fiero.Core;
using System;
using System.Drawing;

namespace Fiero.Business
{
    public static class EntityBuilderExtensions
    {
        public static EntityBuilder<T> WithName<T>(this EntityBuilder<T> builder, string name)
            where T : Entity => builder.AddOrTweak<InfoComponent>(c => c.Name = name);
        public static EntityBuilder<T> WithPosition<T>(this EntityBuilder<T> builder, Coord pos)
            where T : Drawable => builder.AddOrTweak<PhysicsComponent>(c => c.Position = pos);
        public static EntityBuilder<T> WithSprite<T>(this EntityBuilder<T> builder, string sprite, SFML.Graphics.Color? tint = null)
            where T : Drawable => builder.AddOrTweak<RenderComponent>(c => {
                c.SpriteName = sprite;
                c.Sprite.Scale = new(1, 1);
                c.Sprite.Color = tint ?? c.Sprite.Color;
            });
        public static EntityBuilder<T> WithColor<T>(this EntityBuilder<T> builder, SFML.Graphics.Color tint)
            where T : Drawable => builder.AddOrTweak<RenderComponent>(c => {
                c.Sprite.Color = tint;
            });
        public static EntityBuilder<T> WithActorInfo<T>(this EntityBuilder<T> builder, ActorName type, MonsterTierName tier)
            where T : Actor => builder.AddOrTweak<ActorComponent>(c => {
                c.Type = type;
                c.Tier = tier;
            });
        public static EntityBuilder<T> WithMaximumHealth<T>(this EntityBuilder<T> builder, int maximum)
            where T : Actor => builder.AddOrTweak<ActorComponent>(c => c.Health = c.MaximumHealth = maximum);
        public static EntityBuilder<T> WithInventory<T>(this EntityBuilder<T> builder, int capacity)
            where T : Actor => builder.AddOrTweak<InventoryComponent>(c => c.Capacity = capacity);
        public static EntityBuilder<T> WithItems<T>(this EntityBuilder<T> builder, params Item[] items)
            where T : Actor => builder.AddOrTweak<InventoryComponent>(c => {
                c.Capacity = Math.Max(c.Capacity, items.Length);
                foreach (var item in items) {
                    c.TryPut(item);
                }
            });
        public static EntityBuilder<T> WithEquipment<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<EquipmentComponent>();
        public static EntityBuilder<T> WithEnemyAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>(c => {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                c.ActionProvider = new AiActionProvider(gameSystems);
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithPlayerAi<T>(this EntityBuilder<T> builder, GameUI ui)
            where T : Actor => builder.AddOrTweak<ActionComponent>(c => {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                c.ActionProvider = new PlayerActionProvider(ui, gameSystems);
            });
        public static EntityBuilder<T> WithFieldOfView<T>(this EntityBuilder<T> builder, int radius)
            where T : Actor => builder.AddOrTweak<FieldOfViewComponent>(c => {
                c.Radius = radius;
            });
        public static EntityBuilder<T> WithFaction<T>(this EntityBuilder<T> builder, FactionName faction)
            where T : Actor => builder.AddOrTweak<FactionComponent>(c => {
                var factionSystem = (FactionSystem)builder.ServiceFactory.GetInstance(typeof(FactionSystem));
                c.Type = faction;
                c.Relationships = factionSystem.GetRelationships(c.Type);
            });
        public static EntityBuilder<T> WithLogging<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<LogComponent>(_ => { });
        public static EntityBuilder<T> WithNpcInfo<T>(this EntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<NpcComponent>(c => c.Type = type);
        public static EntityBuilder<T> WithDialogueTriggers<T>(this EntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<DialogueComponent>(c => {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                gameSystems.Dialogue.SetTriggers(gameSystems, type, c);
            });
        public static EntityBuilder<T> WithDialogueTriggers<T>(this EntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<DialogueComponent>(c => {
                var gameSystems = (GameSystems)builder.ServiceFactory.GetInstance(typeof(GameSystems));
                gameSystems.Dialogue.SetTriggers(gameSystems, type, c);
            });
        public static EntityBuilder<T> WithPortalInfo<T>(this EntityBuilder<T> builder, FloorConnection conn)
            where T : Feature => builder.AddOrTweak<PortalComponent>(c => {
                c.Connection = conn;
            });
        public static EntityBuilder<T> WithFeatureInfo<T>(this EntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<FeatureComponent>(c => {
                c.Name = type;
                c.BlocksMovement = c.Name switch {
                    FeatureName.Door => true,
                    FeatureName.Shrine => true,
                    FeatureName.Chest => true,
                    _ => false
                };
                c.BlocksLight = c.Name switch {
                    FeatureName.Door => true,
                    _ => false
                };
            });
        public static EntityBuilder<T> WithTileInfo<T>(this EntityBuilder<T> builder, TileName type)
            where T : Tile => builder.AddOrTweak<TileComponent>(c => {
                c.Name = type;
                c.BlocksMovement = type switch {
                    TileName.Debug1 => false,
                    TileName.Debug2 => false,
                    TileName.Debug3 => false,
                    TileName.Ground => false,
                    _ => true
                };
                c.BlocksLight = type switch {
                    TileName.Debug1 => false,
                    TileName.Debug2 => false,
                    TileName.Debug3 => false,
                    TileName.Ground => false,
                    _ => true
                };
            });
        public static EntityBuilder<T> WithConsumableInfo<T>(this EntityBuilder<T> builder, int remainingUses, int maxUses, bool consumable)
            where T : Consumable => builder.AddOrTweak<ConsumableComponent>(c => {
                c.RemainingUses = remainingUses;
                c.MaximumUses = maxUses;
                c.ConsumedWhenEmpty = consumable;
            });
        public static EntityBuilder<T> WithPotionInfo<T>(this EntityBuilder<T> builder, EffectName effect)
            where T : Potion => builder.AddOrTweak<PotionComponent>(c => {
                c.Effect = effect;
            });
        public static EntityBuilder<T> WithScrollInfo<T>(this EntityBuilder<T> builder, EffectName effect)
            where T : Scroll => builder.AddOrTweak<ScrollComponent>(c => {
                c.Effect = effect;
            });
        public static EntityBuilder<T> WithWeaponInfo<T>(this EntityBuilder<T> builder,
            WeaponName type, AttackName attack, WeaponHandednessName hands, int baseDamage, int swingDelay)
            where T : Weapon => builder.AddOrTweak<WeaponComponent>(c => {
                c.Type = type;
                c.AttackType = attack;
                c.Handedness = hands;
                c.BaseDamage = baseDamage;
                c.SwingDelay = swingDelay;
            });
        public static EntityBuilder<T> WithArmorInfo<T>(this EntityBuilder<T> builder, ArmorName type, ArmorSlotName slot)
            where T : Armor => builder.AddOrTweak<ArmorComponent>(c => {
                c.Type = type;
                c.Slot = slot;
            });
        public static EntityBuilder<T> WithItemInfo<T>(this EntityBuilder<T> builder, int rarity, string unidentName)
            where T : Item => builder.AddOrTweak<ItemComponent>(c => {
                c.Rarity = rarity;
                c.UnidentifiedName = unidentName;
            });
    }
}
