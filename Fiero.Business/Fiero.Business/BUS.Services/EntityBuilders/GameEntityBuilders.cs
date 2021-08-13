using Fiero.Core;
using Microsoft.Extensions.Configuration;
using SFML.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameEntityBuilders
    {
        protected readonly GameEntities Entities;
        protected readonly GameGlossaries Glossaries;
        protected readonly GameUI UI;
        protected readonly GameColors<ColorName> Colors;

        public GameEntityBuilders(
            GameEntities entities, 
            GameGlossaries glossaries,
            GameUI ui,
            GameColors<ColorName> colors
        ) {
            Entities = entities;
            Glossaries = glossaries;
            Colors = colors;
            UI = ui;
        }

        public EntityBuilder<Actor> Player
            => Entities.CreateBuilder<Actor>()
            .WithPlayerAi(UI)
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Player))
            .WithSprite(TextureName.Creatures, nameof(Player), ColorName.White)
            .WithActorInfo(ActorName.Player)
            .WithFaction(FactionName.Players)
            .WithEquipment()
            .WithInventory(50)
            .WithSpellLibrary()
            .WithFieldOfView(7)
            .WithLogging()
            .WithBlood(ColorName.Red, 100)
            .WithIntrinsicEffect(() => new AutopickupEffect())
            ;

        private EntityBuilder<Actor> Enemy() 
            => Entities.CreateBuilder<Actor>()
            .WithLogging()
            .WithEnemyAi()
            .WithName(nameof(Enemy))
            .WithSprite(TextureName.Creatures, "None", ColorName.White)
            .WithActorInfo(ActorName.None)
            .WithFaction(FactionName.None)
            .WithPhysics(Coord.Zero)
            .WithInventory(5)
            .WithEquipment()
            .WithFieldOfView(7)
            ;

        public EntityBuilder<Weapon> Weapon(string unidentName, WeaponName type, int baseDamage, int swingDelay, int itemRarity)
            => Entities.CreateBuilder<Weapon>()
            .WithName(type.ToString())
            .WithSprite(TextureName.Items, type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithWeaponInfo(type, baseDamage, swingDelay)
            .WithItemInfo(itemRarity, unidentName)
            ;

        private EntityBuilder<T> Consumable<T>(int itemRarity, int remainingUses, int maxUses, bool consumedWhenEmpty, string unidentName = null)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite(TextureName.Items, "None", ColorName.White)
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty)
            .WithItemInfo(itemRarity, unidentName)
            ;

        private EntityBuilder<T> Throwable<T>(ThrowableName name, int itemRarity, int remainingUses, int maxUses, int damage, int maxRange, ThrowName @throw, string unidentName = null)
            where T : Throwable
            => Consumable<T>(itemRarity, remainingUses, maxUses, true, unidentName)
            .WithThrowableInfo(name, damage, maxRange, @throw)
            .WithName(name.ToString())
            .WithSprite(TextureName.Items, name.ToString(), ColorName.White)
            ;

        private EntityBuilder<T> Resource<T>(ResourceName name, int amount, int? maxAmount = null)
            where T : Resource
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithItemInfo(0, null)
            .WithName(name.ToString())
            .WithSprite(TextureName.Items, name.ToString(), ColorName.White)
            .WithResourceInfo(name, amount, maxAmount ?? amount)
            ;

        private Func<Effect> ResolveEffect(EffectName effect) => effect switch {
            EffectName.Confusion => () => new GrantedOnUse(new Temporary(new ConfusionEffect(), 10)),
            _ => () => throw new NotImplementedException(effect.ToString())
        };

        public EntityBuilder<Spell> Spell(SpellName type, int baseDamage, int castDelay)
            => Entities.CreateBuilder<Spell>()
            .WithName(type.ToString())
            .WithSprite(TextureName.Items, type.ToString(), ColorName.White)
            .WithSpellInfo(type, baseDamage, castDelay)
            ;

        public EntityBuilder<Potion> Potion(EffectName effect)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + effect.GetHashCode());

            var adjectives = new[] {
                "swirling", "warm", "slimy", "dilute", "clear", "foaming", "fizzling",
                "murky", "sedimented", "glittering", "glowing", "cold", "gelatinous",
                "bubbling", "lumpy", "viscous"
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

            return Consumable<Potion>(unidentName: $"{potionColor.Adjective} potion", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
               .WithName($"Potion of {effect}")
               .WithSprite(TextureName.Items, nameof(Potion), potionColor.Color)
               .WithPotionInfo(effect)
               .WithIntrinsicEffect(ResolveEffect(effect))
               ;
           ;
        }

        public EntityBuilder<Scroll> Scroll(EffectName effect)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + (int)effect * 31);
            var label = ScrollLabel();
            return Consumable<Scroll>(unidentName: $"scroll labelled '{label}'", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
            .WithName($"Scroll of {effect}")
            .WithSprite(TextureName.Items, nameof(Scroll), ColorName.Yellow)
            .WithScrollInfo(effect)
            .WithIntrinsicEffect(ResolveEffect(effect))
            ;

            string ScrollLabel()
            {
                var Vowels = "AEIOU".ToCharArray();
                var consonants = "BDFGKLMRSTVZ".ToCharArray();

                var label = rng.Choose(consonants.Concat(Vowels).ToArray()).ToString();
                while(label.Length < 6) {
                    label += GetNextLetter(label);
                }

                return label;
                char GetNextLetter(string previous)
                {
                    if(IsVowel(previous.Last())) {
                        var precedingVowels = 0;
                        foreach (var l in previous.Reverse()) {
                            if (!IsVowel(l)) break;
                            precedingVowels++;
                        }
                        var chanceOfAnotherVowel = Math.Pow(0.25 - previous.Length / 20d, precedingVowels + 1);
                        if(rng.NextDouble() < chanceOfAnotherVowel) {
                            return rng.Choose(Vowels);
                        }
                        return rng.Choose(consonants);
                    }
                    else {
                        var precedingConsonants = 0;
                        foreach (var l in previous.Reverse()) {
                            if (IsVowel(l)) break;
                            precedingConsonants++;
                        }
                        var chanceOfAnotherConsonant = Math.Pow(0.25 + previous.Length / 20d, precedingConsonants + 1);
                        if (rng.NextDouble() < chanceOfAnotherConsonant) {
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
            .WithSprite(TextureName.Features, type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithFeatureInfo(type)
            ;

        private EntityBuilder<Tile> Tile(TileName type, ColorName color)
            => Entities.CreateBuilder<Tile>()
            .WithName(type.ToString())
            .WithSprite(TextureName.Tiles, type.ToString(), color)
            .WithPhysics(Coord.Zero)
            .WithTileInfo(type)
            ;

        #region NPCs
        public EntityBuilder<Actor> NPC_Rat() 
            => Enemy()
            .WithName(nameof(ActorName.Rat))
            .WithActorInfo(ActorName.Rat)
            .WithFaction(FactionName.Rats)
            .WithSprite(TextureName.Creatures, nameof(ActorName.Rat), ColorName.White)
            .WithBlood(ColorName.LightRed, 25)
            ;

        public EntityBuilder<Actor> NPC_Snake() 
            => Enemy()
            .WithName(nameof(ActorName.Snake))
            .WithActorInfo(ActorName.Snake)
            .WithFaction(FactionName.Snakes)
            .WithSprite(TextureName.Creatures, nameof(ActorName.Snake), ColorName.White)
            .WithBlood(ColorName.LightRed, 10)
            ;

        public EntityBuilder<Actor> NPC_Cat() 
            => Enemy()
            .WithName(nameof(ActorName.Cat))
            .WithActorInfo(ActorName.Cat)
            .WithFaction(FactionName.Cats)
            .WithSprite(TextureName.Creatures, nameof(ActorName.Cat), ColorName.White)
            .WithBlood(ColorName.LightRed, 50)
            ;

        public EntityBuilder<Actor> NPC_Dog() 
            => Enemy()
            .WithName(nameof(ActorName.Dog))
            .WithActorInfo(ActorName.Dog)
            .WithFaction(FactionName.Dogs)
            .WithSprite(TextureName.Creatures, nameof(ActorName.Dog), ColorName.White)
            .WithBlood(ColorName.LightRed, 80)
            ;

        public EntityBuilder<Actor> NPC_Boar() 
            => Enemy()
            .WithName(nameof(ActorName.Boar))
            .WithActorInfo(ActorName.Boar)
            .WithFaction(FactionName.Boars)
            .WithSprite(TextureName.Creatures, nameof(ActorName.Boar), ColorName.White)
            .WithBlood(ColorName.LightRed, 200)
            ;
        #endregion

        #region Sentient NPCs
        public EntityBuilder<Actor> NPC_RatKnight()
            => NPC_Rat()
            .WithName("Rat Knight")
            .WithNpcInfo(NpcName.RatKnight)
            .WithDialogueTriggers(NpcName.RatKnight)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatKnight), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatArcher()
            => NPC_Rat()
            .WithName("Rat Archer")
            .WithNpcInfo(NpcName.RatArcher)
            .WithDialogueTriggers(NpcName.RatArcher)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatArcher), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatWizard()
            => NPC_Rat()
            .WithName("Rat Wizard")
            .WithNpcInfo(NpcName.RatWizard)
            .WithDialogueTriggers(NpcName.RatWizard)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatWizard), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatMerchant()
            => NPC_Rat()
            .WithName("Rat Merchant")
            .WithNpcInfo(NpcName.RatMerchant)
            .WithDialogueTriggers(NpcName.RatMerchant)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatMerchant), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatMonk()
            => NPC_Rat()
            .WithName("Rat Monk")
            .WithNpcInfo(NpcName.RatMonk)
            .WithDialogueTriggers(NpcName.RatMonk)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatMonk), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatPugilist()
            => NPC_Rat()
            .WithName("Rat Pugilist")
            .WithNpcInfo(NpcName.RatPugilist)
            .WithDialogueTriggers(NpcName.RatPugilist)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatPugilist), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatThief()
            => NPC_Rat()
            .WithName("Rat Thief")
            .WithNpcInfo(NpcName.RatThief)
            .WithDialogueTriggers(NpcName.RatThief)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatThief), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatOutcast()
            => NPC_Rat()
            .WithName("Rat Outcast")
            .WithNpcInfo(NpcName.RatOutcast)
            .WithDialogueTriggers(NpcName.RatOutcast)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatOutcast), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_RatArsonist()
            => NPC_Rat()
            .WithName("Rat Arsonist")
            .WithNpcInfo(NpcName.RatArsonist)
            .WithDialogueTriggers(NpcName.RatArsonist)
            .WithSprite(TextureName.Creatures, nameof(NpcName.RatArsonist), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_SandSnake()
            => NPC_Snake()
            .WithName("Sand Snake")
            .WithNpcInfo(NpcName.SandSnake)
            .WithDialogueTriggers(NpcName.SandSnake)
            .WithSprite(TextureName.Creatures, nameof(NpcName.SandSnake), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_Cobra()
            => NPC_Snake()
            .WithName("Cobra")
            .WithNpcInfo(NpcName.Cobra)
            .WithDialogueTriggers(NpcName.Cobra)
            .WithSprite(TextureName.Creatures, nameof(NpcName.Cobra), ColorName.White)
            ;
        public EntityBuilder<Actor> NPC_Boa()
            => NPC_Snake()
            .WithName("Boa")
            .WithNpcInfo(NpcName.Boa)
            .WithDialogueTriggers(NpcName.Boa)
            .WithSprite(TextureName.Creatures, nameof(NpcName.Boa), ColorName.White)
            ;
        #endregion

        #region BOSSES
        public EntityBuilder<Actor> Boss_NpcGreatKingRat()
            => NPC_Rat()
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithDialogueTriggers(NpcName.GreatKingRat)
            .WithSprite(TextureName.Creatures, nameof(NpcName.GreatKingRat), ColorName.White)
            ;
        public EntityBuilder<Actor> Boss_NpcKingSerpent()
            => NPC_Rat()
            .WithName("Serpentine King")
            .WithNpcInfo(NpcName.KingSerpent)
            .WithDialogueTriggers(NpcName.KingSerpent)
            .WithSprite(TextureName.Creatures, nameof(NpcName.KingSerpent), ColorName.White)
            ;
        #endregion

        #region WEAPONS
        public EntityBuilder<Weapon> Weapon_Sword()
            => Weapon("sword", WeaponName.Sword, baseDamage: 3, swingDelay: 0, itemRarity: 10)
            ;
        #endregion

        #region THROWABLES
        public EntityBuilder<Throwable> Throwable_Rock(int charges)
            => Throwable<Throwable>(ThrowableName.Rock, itemRarity: 1, remainingUses: charges, maxUses: charges, damage: 4, maxRange: 3, @throw: ThrowName.Arc)
            ;
        #endregion

        #region RESOURCES
        public EntityBuilder<Resource> Resource_Gold(int amount)
            => Resource<Resource>(ResourceName.Gold, amount, maxAmount: 999999)
            ;
        #endregion

        #region FEATURES
        private ColorName GetBranchColor(DungeonBranchName branch) => branch switch {
            DungeonBranchName.Dungeon => ColorName.Gray,
            DungeonBranchName.Sewers => ColorName.Green,
            DungeonBranchName.Kennels => ColorName.Red,
            _ => ColorName.White
        };

        public EntityBuilder<Feature> Feature_Chest()
            => Feature<Feature>(FeatureName.Chest)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = true)
            ;
        public EntityBuilder<Feature> Feature_Shrine()
            => Feature<Feature>(FeatureName.Shrine)
            .WithDialogueTriggers(FeatureName.Shrine)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = true)
            ;
        public EntityBuilder<Feature> Feature_Trap()
            => Feature<Feature>(FeatureName.Trap)
            .WithIntrinsicEffect(() => new TrapEffect())
            .Tweak<RenderComponent>(x => x.Hidden = true)
            ;
        public EntityBuilder<Feature> Feature_Door()
            => Feature<Feature>(FeatureName.Door)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = x.BlocksLight = true)
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
        public EntityBuilder<BloodSplatter> Feature_BloodSplatter(int bloodAmount, ColorName bloodColor = ColorName.Red)
            => Feature<BloodSplatter>(FeatureName.BloodSplatter)
            .WithColor(bloodColor)
            .WithBlood(bloodColor, Math.Min(100, bloodAmount), 100)
            ;
        #endregion

        #region TILES
        public EntityBuilder<Tile> Tile_Wall()
            => Tile(TileName.Wall, ColorName.Gray)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = x.BlocksLight = true)
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
        #endregion
    }
}
