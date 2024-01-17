using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using LightInject;
using Unconcern.Common;
namespace Fiero.Business
{
    public static class EntityBuilderExtensions
    {
        public static EntityBuilder<T> LoadState<T>(this EntityBuilder<T> builder, string resourceName, bool throwOnFail = false)
            where T : Entity
        {
            var scripts = builder.Entities.ServiceFactory.GetInstance<GameScripts<ScriptName>>();
            var entities = (ErgoScript)scripts.Get(ScriptName.Entity);
            var query = entities.VM.ParseAndCompileQuery($"dict('{resourceName}', X)");
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
        public static EntityBuilder<T> WithName<T>(this EntityBuilder<T> builder, string name)
            where T : Entity => builder.AddOrTweak<InfoComponent>((s, c) => c.Name = s.GetInstance<GameResources>().Localizations.Translate(name));
        public static EntityBuilder<T> WithDescription<T>(this EntityBuilder<T> builder, string desc)
            where T : Entity => builder.AddOrTweak<InfoComponent>((s, c) => c.Description = s.GetInstance<GameResources>().Localizations.Translate(desc));
        public static EntityBuilder<T> WithEffectTracking<T>(this EntityBuilder<T> builder)
            where T : Entity => builder.AddOrTweak<EffectsComponent>();
        public static EntityBuilder<T> WithIntrinsicEffect<T>(this EntityBuilder<T> builder, EffectDef def, Func<EffectDef, Effect> wrap = null)
            where T : Entity => builder.AddOrTweak<EffectsComponent>((s, c) =>
            {
                c.Intrinsic.Add(def);
                builder.Built += OnBuilt;
                void OnBuilt(EntityBuilder<T> b, T o)
                {
                    builder.Built -= OnBuilt;
                    var systems = s.GetInstance<MetaSystem>();
                    var sub = new Subscription(throwOnDoubleDispose: false);
                    wrap ??= (e => e.Resolve(o));
                    var fx = wrap(def);
                    fx.Start(systems, o);
                    sub.Dispose();
                }
            });
        public static EntityBuilder<T> WithPhysics<T>(this EntityBuilder<T> builder, Coord pos, bool canMove = false, bool blocksMovement = false, bool blocksLight = false)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>((s, c) =>
            {
                c.Position = pos;
                c.BlocksLight = blocksMovement;
                c.BlocksLight = blocksLight;
                c.CanMove = canMove;
            });
        public static EntityBuilder<T> WithPosition<T>(this EntityBuilder<T> builder, Coord pos, FloorId? floorId = null)
            where T : PhysicalEntity => builder.AddOrTweak<PhysicsComponent>((s, c) =>
            {
                c.Position = pos;
                if (floorId.HasValue)
                {
                    c.FloorId = floorId.Value;
                }
            });
        public static EntityBuilder<T> WithSprite<T>(this EntityBuilder<T> builder, RenderLayerName layer, TextureName texture, string sprite, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>((s, c) =>
            {
                c.Layer = layer;
                c.Texture = texture;
                c.Sprite = sprite;
                c.Color = color;
            });
        public static EntityBuilder<T> WithColor<T>(this EntityBuilder<T> builder, ColorName color)
            where T : DrawableEntity => builder.AddOrTweak<RenderComponent>((s, c) =>
            {
                c.Color = color;
            });
        public static EntityBuilder<T> WithActorInfo<T>(this EntityBuilder<T> builder, ActorName type)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static EntityBuilder<T> WithRace<T>(this EntityBuilder<T> builder, RaceName race)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Race = race;
            });
        public static EntityBuilder<T> WithHealth<T>(this EntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Health = new(0, maximum, current ?? maximum);
            });
        public static EntityBuilder<T> WithMagic<T>(this EntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Magic = new(0, maximum, current ?? maximum);
            });
        public static EntityBuilder<T> WithExperience<T>(this EntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Experience = new(0, maximum, current ?? maximum);
            });
        public static EntityBuilder<T> WithLevel<T>(this EntityBuilder<T> builder, int maximum, int? current = null)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Level = new(0, maximum, current ?? 0);
            });
        public static EntityBuilder<T> WithCorpse<T>(this EntityBuilder<T> builder, CorpseName type, Chance chance)
            where T : Actor => builder.AddOrTweak<ActorComponent>((s, c) =>
            {
                c.Corpse = new(type, chance);
            });
        public static EntityBuilder<T> WithInventory<T>(this EntityBuilder<T> builder, int capacity)
            where T : Actor => builder.AddOrTweak<InventoryComponent>((s, c) => c.Capacity = capacity);
        public static EntityBuilder<T> WithItems<T>(this EntityBuilder<T> builder, params Item[] items)
            where T : PhysicalEntity => builder.AddOrTweak<InventoryComponent>((s, c) =>
            {
                c.Capacity = Math.Max(c.Capacity, items.Length);
                // Delegate the actual adding of each item to when the owner of the inventory is spawned
                builder.Built += OnBuilt;
                void OnBuilt(EntityBuilder<T> b, T e)
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
        public static EntityBuilder<T> WithSpellLibrary<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>();
        public static EntityBuilder<T> WithSpells<T>(this EntityBuilder<T> builder, params Spell[] spells)
            where T : Actor => builder.AddOrTweak<SpellLibraryComponent>((s, c) =>
            {
                // Delegate the actual adding of each spell to when the owner of the library is spawned
                builder.Built += OnBuilt;
                void OnBuilt(EntityBuilder<T> b, T e)
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
        public static EntityBuilder<T> WithActorEquipment<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActorEquipmentComponent>();
        public static EntityBuilder<T> WithIdleAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<IdleActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithEnemyAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<AiActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithAutoPlayerAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<AutoPlayerActionProvider>();
            })
                .AddOrTweak<AiComponent>();
        public static EntityBuilder<T> WithLikedItems<T>(this EntityBuilder<T> builder, params Func<Item, bool>[] likedItems)
            where T : Actor => builder.AddOrTweak<AiComponent>((s, c) =>
            {
                c.LikedItems.AddRange(likedItems);
            });
        public static EntityBuilder<T> WithDislikedItems<T>(this EntityBuilder<T> builder, params Func<Item, bool>[] dislikedItems)
            where T : Actor => builder.AddOrTweak<AiComponent>((s, c) =>
            {
                c.DislikedItems.AddRange(dislikedItems);
            });
        public static EntityBuilder<T> WithPlayerAi<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<ActionComponent>((s, c) =>
            {
                c.ActionProvider = s.GetInstance<PlayerActionProvider>();
            });
        public static EntityBuilder<T> WithFieldOfView<T>(this EntityBuilder<T> builder, int radius)
            where T : Actor => builder.AddOrTweak<FieldOfViewComponent>((s, c) =>
            {
                c.Radius = radius;
            });
        public static EntityBuilder<T> WithFaction<T>(this EntityBuilder<T> builder, FactionName faction)
            where T : Actor => builder.AddOrTweak<FactionComponent>((s, c) =>
            {
                c.Name = faction;
            });
        public static EntityBuilder<T> WithParty<T>(this EntityBuilder<T> builder, Actor leader = null)
            where T : Actor => builder.AddOrTweak<PartyComponent>((s, c) =>
            {
                c.Leader = leader;
            });
        public static EntityBuilder<T> WithLogging<T>(this EntityBuilder<T> builder)
            where T : Actor => builder.AddOrTweak<LogComponent>();
        public static EntityBuilder<T> WithNpcInfo<T>(this EntityBuilder<T> builder, NpcName type)
            where T : Actor => builder.AddOrTweak<NpcComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static EntityBuilder<T> WithPortalInfo<T>(this EntityBuilder<T> builder, FloorConnection conn)
            where T : Feature => builder.AddOrTweak<PortalComponent>((s, c) =>
            {
                c.Connection = conn;
            });
        public static EntityBuilder<T> WithFeatureInfo<T>(this EntityBuilder<T> builder, FeatureName type)
            where T : Feature => builder.AddOrTweak<FeatureComponent>((s, c) =>
            {
                c.Name = type;
            });
        public static EntityBuilder<T> WithTileInfo<T>(this EntityBuilder<T> builder, TileName type)
            where T : Tile => builder.AddOrTweak<TileComponent>((s, c) =>
            {
                c.Name = type;
            });
        public static EntityBuilder<T> WithConsumableInfo<T>(this EntityBuilder<T> builder, int remainingUses, int maxUses, bool consumable)
            where T : Consumable => builder.AddOrTweak<ConsumableComponent>((s, c) =>
            {
                c.RemainingUses = remainingUses;
                c.MaximumUses = maxUses;
                c.ConsumedWhenEmpty = consumable;
            });
        public static EntityBuilder<T> WithThrowableInfo<T>(this EntityBuilder<T> builder, ThrowableName name, int damage, int maxRange, float mulchChance, bool throwsUseCharges, ThrowName @throw)
            where T : Throwable => builder.AddOrTweak<ThrowableComponent>((s, c) =>
            {
                c.Name = name;
                c.BaseDamage = damage;
                c.MaximumRange = maxRange;
                c.MulchChance = mulchChance;
                c.ThrowsUseCharges = throwsUseCharges;
                c.Throw = @throw;
            });
        public static EntityBuilder<T> WithResourceInfo<T>(this EntityBuilder<T> builder, ResourceName name, int amount, int maxAmount)
            where T : Resource => builder.AddOrTweak<ResourceComponent>((s, c) =>
            {
                c.Name = name;
                c.Amount = amount;
                c.MaximumAmount = maxAmount;
            });
        public static EntityBuilder<T> WithPotionInfo<T>(this EntityBuilder<T> builder, EffectDef quaffEffect, EffectDef throwEffect)
            where T : Potion => builder.AddOrTweak<PotionComponent>((s, c) =>
            {
                c.QuaffEffect = quaffEffect;
                c.ThrowEffect = throwEffect;
            });
        public static EntityBuilder<T> WithFoodInfo<T>(this EntityBuilder<T> builder, EffectDef eatEffect)
            where T : Food => builder.AddOrTweak<FoodComponent>((s, c) =>
            {
                c.EatEffect = eatEffect;
            });
        public static EntityBuilder<T> WithWandInfo<T>(this EntityBuilder<T> builder, EffectDef effect)
            where T : Wand => builder.AddOrTweak<WandComponent>((s, c) =>
            {
                c.Effect = effect;
            });
        public static EntityBuilder<T> WithScrollInfo<T>(this EntityBuilder<T> builder, EffectDef effect, ScrollModifierName modifier)
            where T : Scroll => builder.AddOrTweak<ScrollComponent>((s, c) =>
            {
                c.Effect = effect;
                c.Modifier = modifier;
            });
        public static EntityBuilder<T> WithSpellInfo<T>(this EntityBuilder<T> builder, TargetingShape shape, SpellName effect, int damage, int delay)
            where T : Spell => builder.AddOrTweak<SpellComponent>((s, c) =>
            {
                c.Name = effect;
                c.TargetingShape = shape;
                c.TargetingFilter = (_, __, ___) => true;
                c.BaseDamage = damage;
                c.CastDelay = delay;
            });
        public static EntityBuilder<T> WithTargetingShape<T>(this EntityBuilder<T> builder, TargetingShape shape)
            where T : Spell => builder.Tweak<SpellComponent>((s, c) =>
            {
                c.TargetingShape = shape;
            });
        public static EntityBuilder<T> WithTargetingFilter<T>(this EntityBuilder<T> builder, Func<MetaSystem, Actor, PhysicalEntity, bool> targetFilter)
            where T : Spell => builder.Tweak<SpellComponent>((s, c) =>
            {
                c.TargetingFilter = targetFilter;
            });

        public static EntityBuilder<T> WithWeaponInfo<T>(this EntityBuilder<T> builder, WeaponName type, int baseDamage, int swingDelay)
            where T : Weapon => builder.AddOrTweak<WeaponComponent>((s, c) =>
            {
                c.Type = type;
                c.BaseDamage = baseDamage;
                c.SwingDelay = swingDelay;
            });
        public static EntityBuilder<T> WithCorpseInfo<T>(this EntityBuilder<T> builder, CorpseName type)
            where T : Corpse => builder.AddOrTweak<CorpseComponent>((s, c) =>
            {
                c.Type = type;
            });
        public static EntityBuilder<T> WithItemInfo<T>(this EntityBuilder<T> builder, int rarity, string unidentName = null)
            where T : Item => builder.AddOrTweak<ItemComponent>((s, c) =>
            {
                c.Rarity = rarity;
                if (unidentName != null)
                    c.UnidentifiedName = s.GetInstance<GameResources>().Localizations.Translate(unidentName);
                c.Identified = String.IsNullOrEmpty(unidentName);
            });
        public static EntityBuilder<T> WithEquipmentInfo<T>(this EntityBuilder<T> builder, EquipmentTypeName type)
            where T : Equipment => builder.AddOrTweak<EquipmentComponent>((s, c) =>
            {
                c.Type = type;
            });
        //public static EntityBuilder<T> WithScriptInfo<T>(this EntityBuilder<T> builder, string fileName, bool trace = false, bool cache = false)
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
        public static EntityBuilder<T> WithTraitTracking<T>(this EntityBuilder<T> builder)
            where T : Entity => builder.AddOrTweak<TraitsComponent>();
        public static EntityBuilder<T> WithIntrinsicTrait<T>(this EntityBuilder<T> builder, Trait trait)
            where T : Entity => builder.AddOrTweak<TraitsComponent>((s, c) =>
            {
                c.AddIntrinsicTrait(trait);
            })
            .WithIntrinsicEffect(trait.Effect);
    }
}
