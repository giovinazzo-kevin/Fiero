namespace Fiero.Business
{
    [SingletonDependency]
    public class GameEntityBuilders
    {
        protected readonly GameEntities Entities;
        protected readonly GameUI UI;
        protected readonly GameColors<ColorName> Colors;
        protected readonly GameScripts<ScriptName> Scripts;

        public GameEntityBuilders(
            GameEntities entities,
            GameUI ui,
            GameColors<ColorName> colors,
            GameScripts<ScriptName> scripts
        )
        {
            Entities = entities;
            Colors = colors;
            Scripts = scripts;
            UI = ui;
        }


        public EntityBuilder<Actor> Dummy(TextureName texture, string sprite, string name = null, ColorName? tint = null, bool solid = false)
            => Entities.CreateBuilder<Actor>()
            .AddOrTweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = solid)
            .WithHealth(1)
            .WithActorInfo(ActorName.None)
            .WithFaction(FactionName.None)
            .WithIdleAi()
            .WithName(name ?? sprite)
            .WithSprite(RenderLayerName.Features, texture, sprite, tint ?? ColorName.White)
            .WithFieldOfView(0)
            .WithEffectTracking()
            .WithTraitTracking()
            ;

        //public EntityBuilder<Script> Script(string scriptPath, string name = null, bool trace = false, bool cache = true)
        //    => Entities.CreateBuilder<Script>()
        //    .WithName(name ?? scriptPath)
        //    .WithScriptInfo(scriptPath, trace: trace, cache: cache)
        //    .WithEffectTracking()
        //    ;

        public EntityBuilder<Actor> Player()
            => Entities.CreateBuilder<Actor>()
            .WithPlayerAi()
            .WithHealth(maximum: 1, current: 1)
            .WithMagic(maximum: 1, current: 1)
            .WithLevel(maximum: 99, current: 0)
            .WithExperience(maximum: 100, current: 0)
            .WithPhysics(Coord.Zero, canMove: true)
            .WithName(nameof(Player))
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(Player), ColorName.White)
            .WithActorInfo(ActorName.Player)
            .WithFaction(FactionName.Players)
            .WithRace(RaceName.Monster)
            .WithActorEquipment()
            .WithInventory(50)
            .WithSpellLibrary()
            .WithFieldOfView(7)
            .WithLogging()
            .WithEffectTracking()
            .WithIntrinsicEffect(new(EffectName.AutoPickup, canStack: false))
            .WithDislikedItems(i => i.TryCast<Corpse>(out _))
            .WithParty()
            .WithTraitTracking()
            ;

        private EntityBuilder<Actor> Enemy()
            => Entities.CreateBuilder<Actor>()
            .WithInventory(0)
            .WithLogging()
            .WithEnemyAi()
            .WithHealth(1)
            .WithLevel(maximum: 99, current: 0)
            .WithName(nameof(Enemy))
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, "None", ColorName.White)
            .WithActorInfo(ActorName.Monster)
            .WithRace(RaceName.Monster)
            .WithNpcInfo(NpcName.Monster)
            .WithFaction(FactionName.Monsters)
            .WithPhysics(Coord.Zero, canMove: true)
            .WithActorEquipment()
            .WithFieldOfView(5)
            .WithEffectTracking()
            .WithDislikedItems(i => i.TryCast<Corpse>(out _))
            .WithParty()
            .WithTraitTracking()
            ;

        public EntityBuilder<T> Equipment<T>(EquipmentTypeName type)
            where T : Equipment => Entities.CreateBuilder<T>()
            .WithEquipmentInfo(type)
            ;

        public EntityBuilder<Weapon> Weapon(string unidentName, WeaponName type, int baseDamage, int swingDelay, int itemRarity, bool twoHanded)
            => Equipment<Weapon>(twoHanded ? EquipmentTypeName.Weapon2H : EquipmentTypeName.Weapon1H)
            .WithName($"$Item.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.Transparent)
            .WithPhysics(Coord.Zero)
            .WithWeaponInfo(type, baseDamage, swingDelay)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<Corpse> Corpse(CorpseName type)
            => Entities.CreateBuilder<Corpse>()
            .WithPhysics(Coord.Zero)
            .WithName($"$Corpse.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.White)
            .WithCorpseInfo(type)
            .WithItemInfo(0)
            ;

        private EntityBuilder<T> Consumable<T>(int itemRarity, int remainingUses, int maxUses, bool consumedWhenEmpty, string unidentName = null)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite(RenderLayerName.Items, TextureName.Items, "None", ColorName.White)
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty)
            .WithItemInfo(itemRarity, unidentName)
            ;

        private EntityBuilder<T> Throwable<T>(ThrowableName name, int itemRarity, int remainingUses, int maxUses, int damage, int maxRange, float mulchChance, bool throwsUseCharges, bool consumedWhenEmpty, ThrowName @throw, string unidentName = null)
            where T : Throwable
            => Consumable<T>(itemRarity, remainingUses, maxUses, consumedWhenEmpty, unidentName)
            .WithThrowableInfo(name, damage, maxRange, mulchChance, throwsUseCharges, @throw)
            .WithName($"$Item.{name}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
            ;

        private EntityBuilder<T> Resource<T>(ResourceName name, int amount, int? maxAmount = null)
            where T : Resource
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithItemInfo(0, null)
            .WithName($"$Item.{name}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
            .WithResourceInfo(name, amount, maxAmount ?? amount)
            ;

        public EntityBuilder<Spell> Spell(SpellName type, TargetingShape shape, int baseDamage, int castDelay, EffectDef castEffect)
            => Entities.CreateBuilder<Spell>()
            .WithName($"Spell.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.White)
            .WithSpellInfo(shape, type, baseDamage, castDelay)
            ;

        public EntityBuilder<Potion> Potion(EffectDef quaffEffect, EffectDef throwEffect)
        {
            var rng = Rng.SeededRandom(UI.Store.Get(Data.Global.RngSeed) + 31 * (quaffEffect.GetHashCode() + 17 + throwEffect.GetHashCode()));

            var adjectives = new[] {
                "Swirling", "Warm", "Slimy", "Dilute", "Clear", "Foaming", "Fizzling",
                "Murky", "Sedimented", "Glittering", "Glowing", "Cold", "Gelatinous",
                "Bubbling", "Lumpy", "Viscous"
            };
            var colors = new[] {
                ColorName.Red,
                ColorName.Green,
                ColorName.Blue,
                ColorName.Cyan,
                ColorName.Yellow,
                ColorName.Magenta,
                ColorName.LightRed,
                ColorName.LightGreen,
                ColorName.LightBlue,
                ColorName.LightCyan,
                ColorName.LightYellow,
                ColorName.LightMagenta
            };
            var potionColor = (Adjective: rng.Choose(adjectives), Color: rng.Choose(colors));

            return Throwable<Potion>(
                @throw: ThrowName.Arc,
                name: ThrowableName.Misc,
                damage: 1,
                maxRange: 4,
                mulchChance: 1,
                unidentName: $"$Descriptor.Potion.{potionColor.Adjective}$ $Color.{potionColor.Color}$ $Item.Potion$",
                itemRarity: 1,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false
                )
               .WithName($"$Item.PotionOf$ $Effect.{quaffEffect}$")
               .WithSprite(RenderLayerName.Items, TextureName.Items, nameof(Potion), potionColor.Color)
               .WithPotionInfo(quaffEffect, throwEffect)
               .WithIntrinsicEffect(quaffEffect, e => new GrantedOnQuaff(e))
               .WithIntrinsicEffect(throwEffect, e => new GrantedWhenHitByThrownItem(e))
               ;
        }

        public EntityBuilder<Food> Food(FoodName name, EffectDef eatEffect)
        {
            return Throwable<Food>(
                @throw: ThrowName.Line,
                name: ThrowableName.Misc,
                damage: 0,
                maxRange: 4,
                mulchChance: 0.75f,
                itemRarity: 1,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false
                )
               .WithName($"{name}")
               .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
               .WithFoodInfo(eatEffect)
               .WithIntrinsicEffect(eatEffect, e => new GrantedOnQuaff(e))
               ;
        }


        public EntityBuilder<Wand> Wand(EffectDef effect, int charges)
        {
            var rng = Rng.SeededRandom(UI.Store.Get(Data.Global.RngSeed) + 3 * (effect.GetHashCode() + 41));

            var adjectives = new[] {
                "crooked", "rotten", "engraved", "carved", "gnarled", "twisted", "long",
                "short", "chipped", "simple", "heavy", "knobbly", "rough", "smooth",
                "plain", "straight", "curved", "knotty", "fibrous", "rigid", "flexible",
                "bifurcated", "telescopic", "ergonomic", "ribbed"

            };
            var colors = new[] {
                ColorName.LightRed,
                ColorName.LightGreen,
                ColorName.LightBlue,
                ColorName.LightCyan,
                ColorName.LightYellow,
                ColorName.LightMagenta,
                ColorName.White

            };
            var wandColor = (Adjective: rng.Choose(adjectives), Color: rng.Choose(colors));

            return Throwable<Wand>(
                @throw: ThrowName.Line,
                name: ThrowableName.Misc,
                damage: 1,
                maxRange: 7,
                mulchChance: .75f,
                unidentName: $"$Descriptor.Wand.{wandColor.Adjective}$ $Color.{wandColor.Color}$ $Item.Wand$",
                itemRarity: 1,
                remainingUses: charges,
                maxUses: charges,
                consumedWhenEmpty: false,
                throwsUseCharges: false
                )
               .WithName($"$Item.WandOf$ $Effect.{effect}$")
               .WithSprite(RenderLayerName.Items, TextureName.Items, nameof(Wand), wandColor.Color)
               .WithWandInfo(effect)
               .WithIntrinsicEffect(effect, e => new GrantedWhenHitByZappedWand(e))
               .WithIntrinsicEffect(effect, e => new GrantedWhenHitByThrownItem(e))
               ;
        }

        public EntityBuilder<Scroll> Scroll(EffectDef effect, ScrollModifierName modifier)
        {
            var rng = Rng.SeededRandom(UI.Store.Get(Data.Global.RngSeed) + 23 * (effect.GetHashCode() + 13));
            var label = ScrollLabel();
            var colors = new[] {
                ColorName.Red,
                ColorName.Green,
                ColorName.Blue,
                ColorName.Cyan,
                ColorName.Yellow,
                ColorName.Magenta,
                ColorName.LightRed,
                ColorName.LightGreen,
                ColorName.LightBlue,
                ColorName.LightCyan,
                ColorName.LightYellow,
                ColorName.LightMagenta
            };
            var scrollColor = rng.Choose(colors);
            return Throwable<Scroll>(
                @throw: ThrowName.Line,
                name: ThrowableName.Misc,
                unidentName: $"scroll labelled '{label}'",
                damage: 1,
                maxRange: 10,
                mulchChance: 0,
                itemRarity: 1,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false
            )
               .WithName($"$Item.ScrollOf$ $Effect.{effect}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, nameof(Scroll), scrollColor)
            .WithScrollInfo(effect, modifier)
            .WithIntrinsicEffect(effect, e => new GrantedWhenTargetedByScroll(e, modifier))
            ;

            string ScrollLabel()
            {
                var Vowels = "AEIOU".ToCharArray();
                var consonants = "BDFGKLMRSTVZ".ToCharArray();

                var label = rng.Choose(consonants.Concat(Vowels).ToArray()).ToString();
                while (label.Length < 6)
                {
                    label += GetNextLetter(label);
                }

                return label;
                char GetNextLetter(string previous)
                {
                    if (IsVowel(previous.Last()))
                    {
                        var precedingVowels = 0;
                        foreach (var l in previous.Reverse())
                        {
                            if (!IsVowel(l)) break;
                            precedingVowels++;
                        }
                        var chanceOfAnotherVowel = Math.Pow(0.25 - previous.Length / 20d, precedingVowels + 1);
                        if (rng.NextDouble() < chanceOfAnotherVowel)
                        {
                            return rng.Choose(Vowels);
                        }
                        return rng.Choose(consonants);
                    }
                    else
                    {
                        var precedingConsonants = 0;
                        foreach (var l in previous.Reverse())
                        {
                            if (IsVowel(l)) break;
                            precedingConsonants++;
                        }
                        var chanceOfAnotherConsonant = Math.Pow(0.25 + previous.Length / 20d, precedingConsonants + 1);
                        if (rng.NextDouble() < chanceOfAnotherConsonant)
                        {
                            return rng.Choose(consonants);
                        }
                        return rng.Choose(Vowels);
                    }
                    bool IsVowel(char c) => Vowels.Contains(c);
                }
            }
        }

        private EntityBuilder<TFeature> Feature<TFeature>(FeatureName type)
            where TFeature : Feature
            => Entities.CreateBuilder<TFeature>()
            .WithName(type.ToString())
            .WithSprite(RenderLayerName.Features, TextureName.Features, type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithFeatureInfo(type)
            ;

        private EntityBuilder<Tile> Tile(TileName type, ColorName color)
            => Entities.CreateBuilder<Tile>()
            .WithName(type.ToString())
            .WithSprite(RenderLayerName.Ground, TextureName.Tiles, type.ToString(), color)
            .WithPhysics(Coord.Zero)
            .WithTileInfo(type)
            ;

        #region NPCs
        public EntityBuilder<Actor> NPC_Rat()
            => Enemy()
            .WithInventory(5)
            .WithHealth(3)
            .WithName(nameof(NpcName.Rat))
            .WithRace(RaceName.Rat)
            .WithNpcInfo(NpcName.Rat)
            .WithFaction(FactionName.Rats)
            .WithCorpse(CorpseName.RatCorpse, chance: Chance.FiftyFifty)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Rat), ColorName.White)
            ;

        public EntityBuilder<Actor> NPC_Snake()
            => Enemy()
            .WithName(nameof(NpcName.Snake))
            .WithRace(RaceName.Snake)
            .WithNpcInfo(NpcName.Snake)
            .WithFaction(FactionName.Snakes)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Snake), ColorName.White)
            ;
        #endregion

        protected Item[] Loadout<T>(params (EntityBuilder<T> Item, Chance Chance)[] options)
            where T : Item
        {
            return Inner().ToArray();
            IEnumerable<T> Inner()
            {
                foreach (var (item, chance) in options)
                {
                    if (chance.Check())
                        yield return item.Build();
                }
            }
        }

        #region Sentient NPCs
        public EntityBuilder<Actor> NPC_RatKnight()
            => NPC_Rat()
            .WithHealth(20)
            .WithName("Rat Knight")
            .WithNpcInfo(NpcName.RatKnight)
            .WithDialogueTriggers(NpcName.RatKnight)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatKnight), ColorName.White)
            .WithItems(Loadout(
                (Weapon_Sword(), Chance.Always)
            ))
            .WithLikedItems(
                i => i.TryCast<Weapon>(out _),
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            ;
        public EntityBuilder<Actor> NPC_RatArcher()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Archer")
            .WithNpcInfo(NpcName.RatArcher)
            .WithDialogueTriggers(NpcName.RatArcher)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatArcher), ColorName.White)
            .WithItems(Loadout(
                (Throwable_Rock(Rng.Random.Between(4, 10)), Chance.Always)
            ))
            .WithLikedItems(
                i => i.TryCast<Throwable>(out var throwable) && throwable.ThrowableProperties.ThrowsUseCharges,
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            ;
        public EntityBuilder<Actor> NPC_RatWizard()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Wizard")
            .WithNpcInfo(NpcName.RatWizard)
            .WithDialogueTriggers(NpcName.RatWizard)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatWizard), ColorName.White)
            .WithItems(Loadout(
                Rng.Random.Choose(new[]{
                    (Wand_OfConfusion(Rng.Random.Between(3, 7)), Chance.Always),
                    (Wand_OfEntrapment(Rng.Random.Between(3, 7)), Chance.Always),
                    (Wand_OfTeleport(Rng.Random.Between(3, 7)), Chance.Always)
                })
            ))
            .WithLikedItems(
                i => i.TryCast<Wand>(out _),
                i => i.TryCast<Scroll>(out _),
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            ;
        public EntityBuilder<Actor> NPC_RatMerchant()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Merchant")
            .WithNpcInfo(NpcName.RatMerchant)
            .WithDialogueTriggers(NpcName.RatMerchant)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatMerchant), ColorName.White)
            .WithLikedItems(
                i => true
            )
            ;
        public EntityBuilder<Actor> NPC_RatMonk()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Monk")
            .WithNpcInfo(NpcName.RatMonk)
            .WithDialogueTriggers(NpcName.RatMonk)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatMonk), ColorName.White)
            .WithItems(Loadout(
                (Potion_OfHealing().Tweak<ItemComponent>((s, c) => c.Identified = true), Chance.Always)
            ))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Heal) ?? false
            )
            ;
        public EntityBuilder<Actor> NPC_RatCultist()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Cultist")
            .WithNpcInfo(NpcName.RatCultist)
            .WithDialogueTriggers(NpcName.RatCultist)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatCultist), ColorName.White)
            //.WithItems(Loadout(

            //))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Heal) ?? false
            )
            ;
        public EntityBuilder<Actor> NPC_RatPugilist()
            => NPC_Rat()
            .WithHealth(20)
            .WithName("Rat Pugilist")
            .WithNpcInfo(NpcName.RatPugilist)
            .WithDialogueTriggers(NpcName.RatPugilist)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatPugilist), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatThief()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Thief")
            .WithNpcInfo(NpcName.RatThief)
            .WithDialogueTriggers(NpcName.RatThief)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatThief), ColorName.White)
            .WithLikedItems(
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            ;
        public EntityBuilder<Actor> NPC_RatCheese()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Cheese Enjoyer")
            .WithNpcInfo(NpcName.RatCheese)
            .WithDialogueTriggers(NpcName.RatCheese)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatCheese), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatArsonist()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Arsonist")
            .WithNpcInfo(NpcName.RatArsonist)
            .WithDialogueTriggers(NpcName.RatArsonist)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatArsonist), ColorName.White)
            .WithItems(Loadout(
                (Throwable_Bomb(Rng.Random.Between(1, 3)), Chance.Always)
            ))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Explosion) ?? false
            )
            ;
        public EntityBuilder<Actor> NPC_RatZombie()
            => NPC_Rat()
            .WithHealth(30)
            .WithName("Rat Zombie")
            .WithNpcInfo(NpcName.RatZombie)
            .WithDialogueTriggers(NpcName.RatZombie)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatZombie), ColorName.White)
            .WithCorpse(CorpseName.None, Chance.Never)
            ;
        public EntityBuilder<Actor> NPC_RatSkeleton()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Skeleton")
            .WithNpcInfo(NpcName.RatSkeleton)
            .WithDialogueTriggers(NpcName.RatSkeleton)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatSkeleton), ColorName.White)
            .WithCorpse(CorpseName.None, Chance.Never)
            ;
        public EntityBuilder<Actor> NPC_SandSnake()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Sand Snake")
            .WithNpcInfo(NpcName.SandSnake)
            .WithDialogueTriggers(NpcName.SandSnake)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.SandSnake), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_Cobra()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Cobra")
            .WithNpcInfo(NpcName.Cobra)
            .WithDialogueTriggers(NpcName.Cobra)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Cobra), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_Boa()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Boa")
            .WithNpcInfo(NpcName.Boa)
            .WithDialogueTriggers(NpcName.Boa)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Boa), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_Mimic()
            => Enemy()
            .WithName(nameof(NpcName.Mimic))
            .WithInventory(10)
            .WithHealth(10)
            .WithNpcInfo(NpcName.Mimic)
            .WithDialogueTriggers(NpcName.Mimic)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Mimic), ColorName.White)
            ;
        #endregion

        #region BOSSES
        public EntityBuilder<Actor> Boss_NpcGreatKingRat()
            => NPC_Rat()
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithDialogueTriggers(NpcName.GreatKingRat)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.GreatKingRat), ColorName.White)
            ;
        public EntityBuilder<Actor> Boss_NpcKingSerpent()
            => NPC_Rat()
            .WithName("Serpentine King")
            .WithNpcInfo(NpcName.KingSerpent)
            .WithDialogueTriggers(NpcName.KingSerpent)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.KingSerpent), ColorName.White)
            ;
        #endregion

        #region WEAPONS
        public EntityBuilder<Weapon> Weapon_Sword()
            => Weapon("sword", WeaponName.Sword, baseDamage: 3, swingDelay: 0, itemRarity: 10, twoHanded: false)
            ;
        #endregion

        #region THROWABLES
        public EntityBuilder<Throwable> Throwable_Rock(int charges = 1)
            => Throwable<Throwable>(
                name: ThrowableName.Rock,
                itemRarity: 1,
                remainingUses: charges,
                maxUses: charges,
                damage: 2,
                maxRange: 3,
                mulchChance: 1 / 4f,
                @throw: ThrowName.Arc,
                consumedWhenEmpty: true,
                throwsUseCharges: true
            )
            ;
        public EntityBuilder<Throwable> Throwable_Bomb(int charges = 1, int fuse = 50, int radius = 5)
            => Throwable<Throwable>(
                name: ThrowableName.Bomb,
                itemRarity: 1,
                remainingUses: charges,
                maxUses: 99,
                damage: 0,
                maxRange: 4,
                mulchChance: 1f,
                @throw: ThrowName.Arc,
                consumedWhenEmpty: true,
                throwsUseCharges: true
            )
            .WithIntrinsicEffect(
                EffectDef.FromScript(Scripts.Get(ScriptName.Bomb), $"_{{radius: {radius}, fuse: {fuse}}}"),
                e => new GrantedWhenHitByThrownItem(e))
            ;
        #endregion

        #region FOOD
        #endregion

        #region POTIONS
        public EntityBuilder<Potion> Potion_OfConfusion()
            => Potion(new(EffectName.Confusion, duration: 10, canStack: false), new(EffectName.Confusion, duration: 10));
        public EntityBuilder<Potion> Potion_OfSleep()
            => Potion(new(EffectName.Sleep, duration: 10, canStack: false), new(EffectName.Sleep, duration: 10));
        public EntityBuilder<Potion> Potion_OfSilence()
            => Potion(new(EffectName.Silence, duration: 10, canStack: false), new(EffectName.Silence, duration: 10));
        public EntityBuilder<Potion> Potion_OfEntrapment()
            => Potion(new(EffectName.Entrapment, duration: 10, canStack: false), new(EffectName.Entrapment, duration: 10));
        public EntityBuilder<Potion> Potion_OfTeleport()
            => Potion(new(EffectName.UncontrolledTeleport, canStack: false), new(EffectName.UncontrolledTeleport));
        public EntityBuilder<Potion> Potion_OfHealing()
            => Potion(new(EffectName.Heal, "2"), new(EffectName.Heal, "2"));
        #endregion

        #region SCROLLS
        public EntityBuilder<Scroll> Scroll_OfRaiseUndead()
            => Scroll(new(EffectName.RaiseUndead, UndeadRaisingName.Random.ToString()), ScrollModifierName.AreaAffectsItems);
        public EntityBuilder<Scroll> Scroll_OfMassConfusion()
            => Scroll(new(EffectName.Confusion, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public EntityBuilder<Scroll> Scroll_OfMassSleep()
            => Scroll(new(EffectName.Sleep, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public EntityBuilder<Scroll> Scroll_OfMassSilence()
            => Scroll(new(EffectName.Silence, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public EntityBuilder<Scroll> Scroll_OfMassEntrapment()
            => Scroll(new(EffectName.Entrapment, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public EntityBuilder<Scroll> Scroll_OfMassExplosion()
            => Scroll(new(EffectName.Explosion, "2"), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public EntityBuilder<Scroll> Scroll_OfMagicMapping()
            => Scroll(new(EffectName.MagicMapping, canStack: false), ScrollModifierName.Self);
        #endregion

        #region WANDS
        public EntityBuilder<Wand> Wand_OfConfusion(int charges = 1)
            => Wand(new(EffectName.Confusion, duration: 10), charges);
        public EntityBuilder<Wand> Wand_OfPoison(int magnitude = 1, int charges = 1)
            => Wand(new(EffectName.Poison, magnitude.ToString(), duration: 10), charges);
        public EntityBuilder<Wand> Wand_OfSleep(int charges = 1)
            => Wand(new(EffectName.Sleep, duration: 10), charges);
        public EntityBuilder<Wand> Wand_OfSilence(int charges = 1)
            => Wand(new(EffectName.Silence, duration: 10), charges);
        public EntityBuilder<Wand> Wand_OfEntrapment(int charges = 1)
            => Wand(new(EffectName.Entrapment, duration: 10), charges);
        public EntityBuilder<Wand> Wand_OfTeleport(int charges = 1)
            => Wand(new(EffectName.UncontrolledTeleport), charges);
        #endregion

        #region RESOURCES
        public EntityBuilder<Resource> Resource_Gold(int amount)
            => Resource<Resource>(ResourceName.Gold, amount, maxAmount: 999999)
            ;
        #endregion

        #region FEATURES
        private ColorName GetBranchColor(DungeonBranchName branch) => branch switch
        {
            DungeonBranchName.Dungeon => ColorName.Gray,
            DungeonBranchName.Sewers => ColorName.Green,
            DungeonBranchName.Kennels => ColorName.Red,
            _ => ColorName.White
        };

        private IEnumerable<WeightedItem<Item[]>> ChestLootTable()
        {
            // COMMON CHESTS: Consumables; some thematic, some mixed bags
            yield return new(Loadout(
                RandomPotion()
            ), 1000);
            yield return new(Loadout(
                RandomWand()
            ), 1000);
            yield return new(Loadout(
                (Throwable_Rock(Rng.Random.Between(4, 10)), Chance.Always)
            ), 1000);

            (EntityBuilder<Potion> Item, Chance Chance) RandomPotion() => Rng.Random.Choose(new[] {
                (Potion_OfConfusion(), Chance.Always),
                (Potion_OfHealing(), Chance.Always),
                (Potion_OfSleep(), Chance.Always),
                (Potion_OfTeleport(), Chance.Always),
                (Potion_OfSilence(), Chance.Always),
                (Potion_OfEntrapment(), Chance.Always)
            });

            (EntityBuilder<Wand> Item, Chance Chance) RandomWand() => Rng.Random.Choose(new[] {
                (Wand_OfConfusion(), Chance.Always),
                (Wand_OfPoison(), Chance.Always),
                (Wand_OfSleep(), Chance.Always),
                (Wand_OfTeleport(), Chance.Always),
                (Wand_OfSilence(), Chance.Always),
                (Wand_OfEntrapment(), Chance.Always)
            });
        }
        public EntityBuilder<Feature> Feature_Chest()
            => Feature<Feature>(FeatureName.Chest)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .WithItems(Rng.Random.ChooseWeighted(ChestLootTable().ToArray()))
            ;
        public EntityBuilder<Feature> Feature_Shrine()
            => Feature<Feature>(FeatureName.Shrine)
            .WithDialogueTriggers(FeatureName.Shrine)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            ;
        public EntityBuilder<Feature> Feature_Trap()
            => Feature<Feature>(FeatureName.Trap)
            .WithIntrinsicEffect(new(EffectName.Trap))
            .Tweak<RenderComponent>((s, x) => x.Visibility = VisibilityName.Hidden)
            ;
        public EntityBuilder<Feature> Feature_Door()
            => Feature<Feature>(FeatureName.Door)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksLight = x.BlocksNpcPathing = true)
            ;
        public EntityBuilder<Feature> Feature_SecretDoor(ColorName color = ColorName.Gray)
            => Feature<Feature>(FeatureName.Door)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Wall)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksLight = x.BlocksNpcPathing = true)
            .WithSprite(RenderLayerName.Ground, TextureName.Tiles, TileName.Wall.ToString(), color)
            ;
        public EntityBuilder<Portal> Feature_Downstairs(FloorConnection conn)
            => Feature<Portal>(FeatureName.Downstairs)
            .WithColor(GetBranchColor(conn.To.Branch))
            .WithPortalInfo(conn)
            ;
        public EntityBuilder<Portal> Feature_Upstairs(FloorConnection conn)
            => Feature<Portal>(FeatureName.Upstairs)
            .WithColor(GetBranchColor(conn.From.Branch))
            .WithPortalInfo(conn)
            ;
        #endregion

        #region TILES
        public EntityBuilder<Tile> Tile_Wall()
            => Tile(TileName.Wall, ColorName.Gray)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksLight = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Wall)
            ;
        public EntityBuilder<Tile> Tile_Hole()
            => Tile(TileName.Hole, ColorName.Gray)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Ground)
            ;
        public EntityBuilder<Tile> Tile_Room()
            => Tile(TileName.Room, ColorName.LightGray)
            ;
        public EntityBuilder<Tile> Tile_Corridor()
            => Tile(TileName.Corridor, ColorName.LightGray)
            ;
        public EntityBuilder<Tile> Tile_Unimplemented()
            => Tile(TileName.Error, ColorName.LightMagenta)
            ;
        public EntityBuilder<Tile> Tile_Water()
            => Tile(TileName.Water, ColorName.LightBlue)
            // Moving through water is twice as slow 
            .Tweak<TileComponent>((s, x) => { x.MovementCost = 100; })
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Ground)
            ;
        #endregion
    }
}
