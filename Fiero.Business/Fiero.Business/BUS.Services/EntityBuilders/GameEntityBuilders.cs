using Fiero.Core.Ergo;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameEntityBuilders
    {
        protected readonly GameEntities Entities;
        protected readonly GameUI UI;
        protected readonly GameColors<ColorName> Colors;
        protected readonly GameScripts Scripts;

        public GameEntityBuilders(
            GameEntities entities,
            GameUI ui,
            GameColors<ColorName> colors,
            GameScripts scripts
        )
        {
            Entities = entities;
            Colors = colors;
            Scripts = scripts;
            UI = ui;
        }


        public IEntityBuilder<Actor> Dummy(TextureName texture, string sprite, string name = null, ColorName? tint = null, bool solid = false)
            => Entities.CreateBuilder<Actor>()
            .AddOrTweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = solid)
            .WithHealth(1)
            .WithActorInfo(ActorName.None)
            .WithFaction(FactionName.None)
            .WithIdleAi()
            .WithName(name ?? sprite)
            .WithSprite(RenderLayerName.Actors, texture, sprite, tint ?? ColorName.White)
            .WithFieldOfView(0)
            .WithEffectTracking()
            .WithTraitTracking()
            ;

        public IEntityBuilder<Actor> Dummy_ExplosiveBarrel(int radius = 5)
            => Dummy(TextureName.Features, "ExplosiveBarrel", "Explosive Barrel", ColorName.White, solid: true)
            //.WithFaction(FactionName.Monsters)
            .WithIntrinsicEffect(
                EffectDef.FromScript(Scripts.Get<FieroScript>(ScriptName.Barrel), $"_{{radius: {radius}}}"))
            .Tweak<PhysicsComponent>((_, x) => x.Roots = 1)
            ;

        //public IEntityBuilder<Script> Script(string scriptPath, string name = null, bool trace = false, bool cache = true)
        //    => Entities.CreateBuilder<Script>()
        //    .WithName(name ?? scriptPath)
        //    .WithScriptInfo(scriptPath, trace: trace, cache: cache)
        //    .WithEffectTracking()
        //    ;

        public IEntityBuilder<Actor> Player()
            => Entities.CreateBuilder<Actor>()
            .WithPlayerAi()
            .WithHealth(maximum: 1, current: 1)
            .WithMagic(maximum: 1, current: 1)
            .WithLevel(maximum: 99, current: 0)
            .WithExperience(baseXP: 20)
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
            .WithMoveDelay(100)
            .WithLogging()
            .WithEffectTracking()
            .WithIntrinsicEffect(new(EffectName.AutoPickup, canStack: false))
            .WithLikedItems(i => string.IsNullOrEmpty(i.ItemProperties.OwnerTag))
            .WithDislikedItems(i => i.TryCast<Corpse>(out _))
            .WithParty()
            .WithTraitTracking()
            ;

        private IEntityBuilder<Actor> Enemy()
            => Entities.CreateBuilder<Actor>()
            .WithInventory(0)
            .WithEnemyAi()
            .WithHealth(1)
            .WithLevel(maximum: 99, current: 0)
            .WithExperience(baseXP: 20)
            .WithName(nameof(Enemy))
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, "None", ColorName.White)
            .WithActorInfo(ActorName.Monster)
            .WithRace(RaceName.Monster)
            .WithNpcInfo(NpcName.Monster)
            .WithFaction(FactionName.Monsters)
            .WithPhysics(Coord.Zero, canMove: true)
            .WithActorEquipment()
            .WithFieldOfView(5)
            .WithMoveDelay(100)
            .WithEffectTracking()
            .WithDislikedItems(i => i.TryCast<Corpse>(out _))
            .WithParty()
            .WithTraitTracking()
            ;

        public IEntityBuilder<T> Equipment<T>(EquipmentTypeName type)
            where T : Equipment => Entities.CreateBuilder<T>()
            .WithEquipmentInfo(type)
            ;

        public IEntityBuilder<T> Weapon<T>(string unidentName, WeaponName type, Dice baseDamage, Chance critChance, int swingDelay, int itemRarity, int goldValue)
            where T : Weapon
            => Equipment<T>(EquipmentTypeName.Weapon)
            .WithName($"$Item.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.Transparent)
            .WithPhysics(Coord.Zero)
            .WithWeaponInfo(type, baseDamage, critChance, swingDelay)
            .WithItemInfo(itemRarity, goldValue, unidentName)
            ;

        public IEntityBuilder<Corpse> Corpse(CorpseName type)
            => Entities.CreateBuilder<Corpse>()
            .WithPhysics(Coord.Zero)
            .WithName($"$Corpse.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.White)
            .WithCorpseInfo(type)
            .WithItemInfo(0, 0)
            ;

        private IEntityBuilder<T> Consumable<T>(int itemRarity, int goldValue, int remainingUses, int maxUses, bool consumedWhenEmpty, int chargesConsumedPerUse = 1, string unidentName = null)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite(RenderLayerName.Items, TextureName.Items, "None", ColorName.White)
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty, chargesConsumedPerUse)
            .WithItemInfo(itemRarity, goldValue, unidentName)
            ;

        private IEntityBuilder<T> Projectile<T>(ProjectileName name, int itemRarity, int goldValue, int remainingUses, int maxUses, int damage, int maxRange, float mulchChance, bool throwsUseCharges, bool consumedWhenEmpty, TrajectoryName @throw, bool piercing, bool directional, int chargesConsumedPerUse = 0, string unidentName = null, string trail = null)
            where T : Projectile
            => Consumable<T>(itemRarity, goldValue, remainingUses, maxUses, consumedWhenEmpty, chargesConsumedPerUse, unidentName)
            .WithProjectileInfo(name, damage, maxRange, mulchChance, throwsUseCharges, @throw, piercing, directional)
            .WithName($"$Item.{name}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
            .WithTrailSprite(trail)
            ;

        private IEntityBuilder<T> Resource<T>(ResourceName name, int amount, int? maxAmount = null)
            where T : Resource
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithItemInfo(0, 0)
            .WithName($"$Item.{name}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
            .WithResourceInfo(name, amount, maxAmount ?? amount)
            ;

        public IEntityBuilder<Spell> Spell(SpellName type, TargetingShape shape, int baseDamage, int castDelay, EffectDef castEffect)
            => Entities.CreateBuilder<Spell>()
            .WithName($"Spell.{type}$")
            .WithSprite(RenderLayerName.Items, TextureName.Items, type.ToString(), ColorName.White)
            .WithSpellInfo(shape, type, baseDamage, castDelay)
            ;

        public IEntityBuilder<Potion> Potion(EffectDef quaffEffect, EffectDef throwEffect)
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

            return Projectile<Potion>(
                @throw: TrajectoryName.Arc,
                name: ProjectileName.Misc,
                damage: 1,
                maxRange: 4,
                mulchChance: 1,
                unidentName: $"$Descriptor.Potion.{potionColor.Adjective}$ $Color.{potionColor.Color}$ $Item.Potion$",
                itemRarity: 1,
                goldValue: 50,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false,
                piercing: false,
                directional: false
                )
               .WithName($"$Item.PotionOf$ $Effect.{quaffEffect}$")
               .WithSprite(RenderLayerName.Items, TextureName.Items, nameof(Potion), potionColor.Color)
               .WithPotionInfo(quaffEffect, throwEffect)
               .WithIntrinsicEffect(quaffEffect, e => new GrantedOnQuaff(e))
               .WithIntrinsicEffect(throwEffect, e => new GrantedWhenHitByThrownItem(e))
               ;
        }

        public IEntityBuilder<Food> Food(FoodName name, EffectDef eatEffect)
        {
            return Projectile<Food>(
                @throw: TrajectoryName.Line,
                name: ProjectileName.Misc,
                damage: 0,
                maxRange: 4,
                mulchChance: 0.75f,
                itemRarity: 1,
                goldValue: 20,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false,
                piercing: false,
                directional: false
                )
               .WithName($"{name}")
               .WithSprite(RenderLayerName.Items, TextureName.Items, name.ToString(), ColorName.White)
               .WithFoodInfo(eatEffect)
               .WithIntrinsicEffect(eatEffect, e => new GrantedOnQuaff(e))
               ;
        }


        public IEntityBuilder<Wand> Wand(EffectDef effect, int charges)
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

            return Projectile<Wand>(
                @throw: TrajectoryName.Line,
                name: ProjectileName.Misc,
                damage: 1,
                maxRange: 7,
                mulchChance: .75f,
                unidentName: $"$Descriptor.Wand.{wandColor.Adjective}$ $Color.{wandColor.Color}$ $Item.Wand$",
                itemRarity: 1,
                goldValue: 100,
                remainingUses: charges,
                maxUses: charges,
                consumedWhenEmpty: false,
                throwsUseCharges: false,
                piercing: false,
                directional: false
                )
               .WithName($"$Item.WandOf$ $Effect.{effect}$")
               .WithSprite(RenderLayerName.Items, TextureName.Items, nameof(Wand), wandColor.Color)
               .WithWandInfo(effect)
               .WithIntrinsicEffect(effect, e => new GrantedWhenHitByZappedWand(e))
               .WithIntrinsicEffect(effect, e => new GrantedWhenHitByThrownItem(e))
               ;
        }

        public IEntityBuilder<Scroll> Scroll(EffectDef effect, ScrollModifierName modifier)
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
            return Projectile<Scroll>(
                @throw: TrajectoryName.Line,
                name: ProjectileName.Misc,
                unidentName: $"scroll labelled '{label}'",
                damage: 1,
                maxRange: 10,
                mulchChance: 0,
                itemRarity: 1,
                goldValue: 75,
                remainingUses: 1,
                maxUses: 1,
                consumedWhenEmpty: true,
                throwsUseCharges: false,
                piercing: false,
                directional: false
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

        private IEntityBuilder<TFeature> Feature<TFeature>(FeatureName type)
            where TFeature : Feature
            => Entities.CreateBuilder<TFeature>()
            .WithName(type.ToString())
            .WithSprite(RenderLayerName.Features, TextureName.Features, type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithFeatureInfo(type)
            ;

        private IEntityBuilder<Tile> Tile(TileName type, ColorName color)
            => Entities.CreateBuilder<Tile>()
            .WithName(type.ToString())
            .WithSprite(RenderLayerName.Ground, TextureName.Tiles, type.ToString(), color)
            .WithPhysics(Coord.Zero)
            .WithTileInfo(type)
            ;

        #region NPCs
        public IEntityBuilder<Actor> NPC_Rat()
            => Enemy()
            .WithMoveDelay(200)
            .WithInventory(5)
            .WithHealth(5)
            .WithName(nameof(NpcName.Rat))
            .WithRace(RaceName.Rat)
            .WithNpcInfo(NpcName.Rat)
            .WithFaction(FactionName.Rats)
            .WithCorpse(CorpseName.RatCorpse, chance: Chance.FiftyFifty)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Rat), ColorName.White)
            .LoadState(nameof(NpcName.Rat))
            ;

        public IEntityBuilder<Actor> NPC_Snake()
            => Enemy()
            .WithName(nameof(NpcName.Snake))
            .WithRace(RaceName.Snake)
            .WithNpcInfo(NpcName.Snake)
            .WithFaction(FactionName.Snakes)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Snake), ColorName.White)
            ;
        #endregion

        protected Item[] Loadout(params (IEntityBuilder<Item> Item, Chance Chance)[] options)
        {
            return Inner().ToArray();
            IEnumerable<Item> Inner()
            {
                foreach (var (item, chance) in options)
                {
                    if (chance.Check())
                        yield return (Item)item.Build();
                }
            }
        }

        #region Sentient NPCs
        public IEntityBuilder<Actor> NPC_RatKnight()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Knight")
            .WithNpcInfo(NpcName.RatKnight)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatKnight), ColorName.White)
            .WithItems(Loadout(
                (Weapon_Sword(), Chance.Always)
            ))
            .WithLikedItems(
                i => i.TryCast<Weapon>(out _),
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            .LoadState(nameof(NpcName.RatKnight))
            ;
        public IEntityBuilder<Actor> NPC_RatArcher()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Archer")
            .WithNpcInfo(NpcName.RatArcher)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatArcher), ColorName.White)
            .WithItems(Loadout(
                (Weapon_Bow(), Chance.Always),
                (Projectile_Rock(Rng.Random.Between(4, 10)), Chance.FiftyFifty)
            ))
            .WithLikedItems(
                i => i.TryCast<Projectile>(out var Projectile) && Projectile.ProjectileProperties.ThrowsUseCharges,
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            .LoadState(nameof(NpcName.RatArcher))
            ;
        public IEntityBuilder<Actor> NPC_RatWizard()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Wizard")
            .WithNpcInfo(NpcName.RatWizard)
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
            .LoadState(nameof(NpcName.RatWizard))
            ;
        public IEntityBuilder<Actor> NPC_RatMerchant()
            => NPC_Rat()
            .WithHealth(100)
            .WithName("Rat Merchant")
            .WithNpcInfo(NpcName.RatMerchant)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatMerchant), ColorName.White)
            .WithLikedItems(
                i => true
            )
            .LoadState(nameof(NpcName.RatMerchant))
            ;
        public IEntityBuilder<Actor> NPC_RatMonk()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Monk")
            .WithNpcInfo(NpcName.RatMonk)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatMonk), ColorName.White)
            .WithItems(Loadout(
                (Potion_OfHealing().Tweak<ItemComponent>((s, c) => c.Identified = true), Chance.Always)
            ))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Heal) ?? false
            )
            .LoadState(nameof(NpcName.RatMonk))
            ;
        public IEntityBuilder<Actor> NPC_RatApothecary()
            => NPC_Rat()
            .WithHealth(10)
            .WithName("Rat Apothecary")
            .WithNpcInfo(NpcName.RatApothecary)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatApothecary), ColorName.White)
            //.WithItems(Loadout(

            //))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Heal) ?? false
            )
            .LoadState(nameof(NpcName.RatApothecary))
            ;
        public IEntityBuilder<Actor> NPC_RatPugilist()
            => NPC_Rat()
            .WithHealth(20)
            .WithName("Rat Pugilist")
            .WithNpcInfo(NpcName.RatPugilist)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatPugilist), ColorName.White)
            .LoadState(nameof(NpcName.RatPugilist))
            ;
        public IEntityBuilder<Actor> NPC_RatThief()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Thief")
            .WithNpcInfo(NpcName.RatThief)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatThief), ColorName.White)
            .WithLikedItems(
                i => i.TryCast<Resource>(out var res) && res.ResourceProperties.Name == ResourceName.Gold
            )
            .LoadState(nameof(NpcName.RatThief))
            ;
        public IEntityBuilder<Actor> NPC_RatCheese()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Cheese Enjoyer")
            .WithNpcInfo(NpcName.RatCheese)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatCheese), ColorName.White)
            .LoadState(nameof(NpcName.RatCheese))
            ;
        public IEntityBuilder<Actor> NPC_RatArsonist()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Arsonist")
            .WithNpcInfo(NpcName.RatArsonist)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatArsonist), ColorName.White)
            .WithItems(Loadout(
                (Projectile_Bomb(Rng.Random.Between(1, 3)), Chance.Always)
            ))
            .WithLikedItems(
                i => i.Effects?.Intrinsic.Any(e => e.Name == EffectName.Explosion) ?? false
            )
            .LoadState(nameof(NpcName.RatArsonist))
            ;
        public IEntityBuilder<Actor> NPC_RatZombie()
            => NPC_Rat()
            .WithHealth(30)
            .WithName("Rat Zombie")
            .WithNpcInfo(NpcName.RatZombie)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatZombie), ColorName.White)
            .WithCorpse(CorpseName.None, Chance.Never)
            .LoadState(nameof(NpcName.RatZombie))
            ;
        public IEntityBuilder<Actor> NPC_RatSkeleton()
            => NPC_Rat()
            .WithHealth(15)
            .WithName("Rat Skeleton")
            .WithNpcInfo(NpcName.RatSkeleton)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.RatSkeleton), ColorName.White)
            .WithCorpse(CorpseName.None, Chance.Never)
            .LoadState(nameof(NpcName.RatSkeleton))
            ;
        public IEntityBuilder<Actor> NPC_SandSnake()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Sand Snake")
            .WithNpcInfo(NpcName.SandSnake)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.SandSnake), ColorName.White)
            ;
        public IEntityBuilder<Actor> NPC_Cobra()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Cobra")
            .WithNpcInfo(NpcName.Cobra)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Cobra), ColorName.White)
            ;
        public IEntityBuilder<Actor> NPC_Boa()
            => NPC_Snake()
            .WithHealth(7)
            .WithName("Boa")
            .WithNpcInfo(NpcName.Boa)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Boa), ColorName.White)
            ;
        public IEntityBuilder<Actor> NPC_Mimic()
            => Enemy()
            .WithName(nameof(NpcName.Mimic))
            .WithInventory(10)
            .WithHealth(10)
            .WithNpcInfo(NpcName.Mimic)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.Mimic), ColorName.White)
            ;
        #endregion

        #region BOSSES
        public IEntityBuilder<Actor> NPC_GreatKingRat()
            => NPC_Rat()
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.GreatKingRat), ColorName.White)
            .LoadState(nameof(NpcName.GreatKingRat))
            ;
        public IEntityBuilder<Actor> NPC_KingSerpent()
            => NPC_Rat()
            .WithName("Serpentine King")
            .WithNpcInfo(NpcName.KingSerpent)
            .WithSprite(RenderLayerName.Actors, TextureName.Creatures, nameof(NpcName.KingSerpent), ColorName.White)
            ;
        #endregion

        #region WEAPONS
        public IEntityBuilder<Weapon> Weapon_Sword()
            => Weapon<Weapon>("sword", WeaponName.Sword, baseDamage: new Dice(1, 3), critChance: new(15, 100), swingDelay: 0, itemRarity: 10, goldValue: 100)
            .LoadState(nameof(WeaponName.Sword))
            ;
        public IEntityBuilder<Weapon> Weapon_Dagger()
            => Weapon<Weapon>("dagger", WeaponName.Dagger, baseDamage: new Dice(1, 2), critChance: Chance.Never, swingDelay: 0, itemRarity: 10, goldValue: 100)
            .LoadState(nameof(WeaponName.Dagger))
            ;
        public IEntityBuilder<Weapon> Weapon_Hammer()
            => Weapon<Weapon>("hammer", WeaponName.Hammer, baseDamage: new Dice(2, 3), critChance: Chance.Never, swingDelay: 0, itemRarity: 10, goldValue: 100)
            .LoadState(nameof(WeaponName.Hammer))
            ;
        public IEntityBuilder<Weapon> Weapon_Spear()
            => Weapon<Weapon>("spear", WeaponName.Spear, baseDamage: new Dice(2, 3), critChance: Chance.Never, swingDelay: 0, itemRarity: 10, goldValue: 100)
            .LoadState(nameof(WeaponName.Spear))
            .WithIntrinsicEffect(
                EffectDef.FromScript(Scripts.Get<FieroScript>(ScriptName.Reach), $"_{{range: 1.0}}"),
                e => new GrantedOnEquip(e))
            ;
        public IEntityBuilder<Launcher> Weapon_Bow()
            => Weapon<Launcher>("bow", WeaponName.Bow, baseDamage: new Dice(1, 1), critChance: Chance.Never, swingDelay: 5, itemRarity: 10, goldValue: 100)
            .WithLauncherInfo(Projectile_Arrow())
            .LoadState(nameof(WeaponName.Bow))
            ;
        public IEntityBuilder<Launcher> Weapon_Crossbow()
            => Weapon<Launcher>("crossbow", WeaponName.Crossbow, baseDamage: new Dice(2, 1), critChance: Chance.Never, swingDelay: 5, itemRarity: 10, goldValue: 100)
            .WithLauncherInfo(Projectile_Arrow())
            .LoadState(nameof(WeaponName.Crossbow))
            ;
        #endregion

        #region Projectiles
        public IEntityBuilder<Projectile> Projectile_Rock(int charges = 1)
            => Projectile<Projectile>(
                name: ProjectileName.Rock,
                itemRarity: 1,
                goldValue: 4,
                remainingUses: charges,
                maxUses: Math.Max(charges, 99),
                damage: 2,
                maxRange: 3,
                mulchChance: 1 / 4f,
                @throw: TrajectoryName.Arc,
                consumedWhenEmpty: true,
                throwsUseCharges: true,
                piercing: false,
                directional: false
            )
            ;
        public IEntityBuilder<Projectile> Projectile_Arrow(int charges = 1)
            => Projectile<Projectile>(
                name: ProjectileName.Arrow,
                itemRarity: 1,
                goldValue: 12,
                remainingUses: charges,
                maxUses: charges,
                damage: 2,
                maxRange: 7,
                mulchChance: 1 / 4f,
                @throw: TrajectoryName.Line,
                consumedWhenEmpty: true,
                throwsUseCharges: true,
                piercing: true,
                directional: true
            )
            ;
        public IEntityBuilder<Projectile> Projectile_Grapple()
            => Projectile<Projectile>(
                name: ProjectileName.Grapple,
                itemRarity: 100,
                goldValue: 400,
                remainingUses: 1,
                maxUses: 1,
                damage: 0,
                maxRange: 7,
                mulchChance: 1f,
                @throw: TrajectoryName.Line,
                consumedWhenEmpty: false,
                throwsUseCharges: true,
                chargesConsumedPerUse: 0,
                piercing: false,
                directional: true,
                trail: "Grapple_Trail"
            )
            .WithItemSprite("GrapplingHook")
            .WithIntrinsicEffect(
                EffectDef.FromScript(Scripts.Get<FieroScript>(ScriptName.Grapple)),
                e => new GrantedWhenHitByThrownItem(e))
            ;
        public IEntityBuilder<Projectile> Projectile_Bomb(int charges = 1, int fuse = 3, int radius = 5)
            => Projectile<Projectile>(
                name: ProjectileName.Bomb,
                itemRarity: 1,
                goldValue: 30,
                remainingUses: charges,
                maxUses: 99,
                damage: 0,
                maxRange: 4,
                mulchChance: 1f,
                @throw: TrajectoryName.Arc,
                consumedWhenEmpty: true,
                throwsUseCharges: true,
                piercing: false,
                directional: false
            )
            .WithIntrinsicEffect(
                EffectDef.FromScript(Scripts.Get<FieroScript>(ScriptName.Bomb), $"_{{radius: {radius}, fuse: {fuse}}}"),
                e => new GrantedWhenHitByThrownItem(e))
            ;
        #endregion

        #region FOOD
        #endregion

        #region POTIONS
        public IEntityBuilder<Potion> Potion_OfConfusion()
            => Potion(new(EffectName.Confusion, duration: 10, canStack: false), new(EffectName.Confusion, duration: 10));
        public IEntityBuilder<Potion> Potion_OfSleep()
            => Potion(new(EffectName.Sleep, duration: 10, canStack: false), new(EffectName.Sleep, duration: 10));
        public IEntityBuilder<Potion> Potion_OfSilence()
            => Potion(new(EffectName.Silence, duration: 10, canStack: false), new(EffectName.Silence, duration: 10));
        public IEntityBuilder<Potion> Potion_OfEntrapment()
            => Potion(new(EffectName.Entrapment, duration: 10, canStack: false), new(EffectName.Entrapment, duration: 10));
        public IEntityBuilder<Potion> Potion_OfTeleport()
            => Potion(new(EffectName.UncontrolledTeleport, canStack: false), new(EffectName.UncontrolledTeleport));
        public IEntityBuilder<Potion> Potion_OfHealing()
            => Potion(new(EffectName.Heal, "2"), new(EffectName.Heal, "2"));
        #endregion

        #region SCROLLS
        public IEntityBuilder<Scroll> Scroll_OfRaiseUndead()
            => Scroll(new(EffectName.RaiseUndead, UndeadRaisingName.Random.ToString()), ScrollModifierName.AreaAffectsItems);
        public IEntityBuilder<Scroll> Scroll_OfMassConfusion()
            => Scroll(new(EffectName.Confusion, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public IEntityBuilder<Scroll> Scroll_OfMassSleep()
            => Scroll(new(EffectName.Sleep, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public IEntityBuilder<Scroll> Scroll_OfMassSilence()
            => Scroll(new(EffectName.Silence, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public IEntityBuilder<Scroll> Scroll_OfMassEntrapment()
            => Scroll(new(EffectName.Entrapment, duration: 10, canStack: false), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public IEntityBuilder<Scroll> Scroll_OfMassExplosion()
            => Scroll(new(EffectName.Explosion, "2"), ScrollModifierName.AreaAffectsEveryoneButTarget);
        public IEntityBuilder<Scroll> Scroll_OfMagicMapping()
            => Scroll(new(EffectName.MagicMapping, canStack: false), ScrollModifierName.Self);
        #endregion

        #region WANDS
        public IEntityBuilder<Wand> Wand_OfConfusion(int charges = 1, int duration = 10)
            => Wand(new(EffectName.Confusion, duration: 10), charges);
        public IEntityBuilder<Wand> Wand_OfPoison(int magnitude = 1, int charges = 1, int duration = 10)
            => Wand(new(EffectName.Poison, magnitude.ToString(), duration: duration), charges);
        public IEntityBuilder<Wand> Wand_OfSleep(int charges = 1)
            => Wand(new(EffectName.Sleep, duration: 10), charges);
        public IEntityBuilder<Wand> Wand_OfSilence(int charges = 1)
            => Wand(new(EffectName.Silence, duration: 10), charges);
        public IEntityBuilder<Wand> Wand_OfEntrapment(int charges = 1)
            => Wand(new(EffectName.Entrapment, duration: 10), charges);
        public IEntityBuilder<Wand> Wand_OfTeleport(int charges = 1)
            => Wand(new(EffectName.UncontrolledTeleport), charges);
        #endregion

        #region RESOURCES
        public IEntityBuilder<Resource> Resource_Gold(int amount)
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
            var items = new List<IEntityBuilder<Item>>();
            for (int i = 0; i < new Dice(2, 3).Roll().Sum(); i++)
            {
                items.Add(Rng.Random.ChooseWeighted<IEntityBuilder<Item>>(
                    new(RandomPotion(), 1),
                    new(RandomWand(), 1),
                    new(Projectile_Rock(Rng.Random.Between(4, 10)), 20),
                    new(Resource_Gold(Rng.Random.Between(1, 250)), 50)
                ));
            }

            // COMMON CHESTS: Consumables; some thematic, some mixed bags
            yield return new(Loadout(
                items.Select(x => (x, Chance.Always)).ToArray()
            ), 1000);

            IEntityBuilder<Potion> RandomPotion() => Rng.Random.Choose(new[] {
                Potion_OfConfusion(),
                Potion_OfHealing(),
                Potion_OfSleep(),
                Potion_OfTeleport(),
                Potion_OfSilence(),
                Potion_OfEntrapment()
            });

            IEntityBuilder<Wand> RandomWand() => Rng.Random.Choose(new[] {
                Wand_OfConfusion(),
                Wand_OfPoison(),
                Wand_OfSleep(),
                Wand_OfTeleport(),
                Wand_OfSilence(),
                Wand_OfEntrapment()
            });
        }
        public IEntityBuilder<Feature> Feature_Chest()
            => Feature<Feature>(FeatureName.Chest)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .WithItems(Rng.Random.ChooseWeighted(ChestLootTable().ToArray()))
            .LoadState(nameof(FeatureName.Chest))
            ;
        public IEntityBuilder<Feature> Feature_Shrine()
            => Feature<Feature>(FeatureName.Shrine)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .LoadState(nameof(FeatureName.Shrine))
            ;
        public IEntityBuilder<Feature> Feature_Statue()
            => Feature<Feature>(FeatureName.Statue)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .LoadState(nameof(FeatureName.Statue))
            ;
        public IEntityBuilder<Feature> Feature_Trap()
            => Feature<Feature>(FeatureName.Trap)
            .WithIntrinsicEffect(new(EffectName.Trap))
            .Tweak<RenderComponent>((s, x) => x.Visibility = VisibilityName.Hidden)
            ;
        public IEntityBuilder<Feature> Feature_Door()
            => Feature<Feature>(FeatureName.Door)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Wall)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksLight = x.BlocksNpcPathing = true)
            ;
        public IEntityBuilder<Feature> Feature_SecretDoor(ColorName color = ColorName.Gray)
            => Feature_Door()
            .WithSprite(RenderLayerName.Ground, TextureName.Tiles, TileName.Wall.ToString(), color)
            ;
        public IEntityBuilder<Portal> Feature_Downstairs(FloorConnection conn)
            => Feature<Portal>(FeatureName.Downstairs)
            .WithColor(GetBranchColor(conn.To.Branch))
            .WithPortalInfo(conn)
            ;
        public IEntityBuilder<Portal> Feature_Upstairs(FloorConnection conn)
            => Feature<Portal>(FeatureName.Upstairs)
            .WithColor(GetBranchColor(conn.From.Branch))
            .WithPortalInfo(conn)
            ;
        #endregion

        #region TILES
        public IEntityBuilder<Tile> Tile_Wall()
            => Tile(TileName.Wall, ColorName.Gray)
            .Tweak<PhysicsComponent>((s, x) => x.BlocksMovement = x.BlocksLight = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Wall)
            ;
        public IEntityBuilder<Tile> Tile_Room()
            => Tile(TileName.Room, ColorName.LightGray)
            ;
        public IEntityBuilder<Tile> Tile_Shop()
            => Tile(TileName.Shop, ColorName.LightGray)
            ;
        public IEntityBuilder<Tile> Tile_Corridor()
            => Tile(TileName.Corridor, ColorName.LightGray)
            ;
        public IEntityBuilder<Tile> Tile_Unimplemented()
            => Tile(TileName.Error, ColorName.LightMagenta)
            ;
        public IEntityBuilder<Tile> Tile_Water()
            => Tile(TileName.Water, ColorName.LightBlue)
            .Tweak<PhysicsComponent>((s, x) => x.SwallowsItems = x.SwallowsActors = x.BlocksMovement = x.IsFlat = x.BlocksNpcPathing = x.BlocksPlayerPathing = true)
            .Tweak<RenderComponent>((s, x) => x.Layer = RenderLayerName.Ground)
            .Tweak<TileComponent>((s, x) => { x.MovementCost = 100; })
            ;
        #endregion
    }
}
