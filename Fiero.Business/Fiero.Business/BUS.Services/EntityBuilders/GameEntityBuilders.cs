using Fiero.Core;
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
            .WithSprite(nameof(Player), ColorName.White)
            .WithActorInfo(ActorName.Player)
            .WithFaction(FactionName.Players)
            .WithEquipment()
            .WithInventory(50)
            .WithSpellLibrary()
            .WithFieldOfView(7)
            .WithLogging()
            .WithBlood(ColorName.Red, 100)
            ;

        private EntityBuilder<Actor> Enemy() 
            => Entities.CreateBuilder<Actor>()
            .WithLogging()
            .WithEnemyAi()
            .WithName(nameof(Enemy))
            .WithSprite("None", ColorName.White)
            .WithActorInfo(ActorName.None)
            .WithFaction(FactionName.None)
            .WithPhysics(Coord.Zero)
            .WithInventory(5)
            .WithEquipment()
            .WithFieldOfView(7)
            ;

        public EntityBuilder<Weapon> Weapon(string unidentName, WeaponName type, AttackName attack, WeaponHandednessName hands, int baseDamage, int swingDelay, int itemRarity)
            => Entities.CreateBuilder<Weapon>()
            .WithName(type.ToString())
            .WithSprite(type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithWeaponInfo(type, attack, hands, baseDamage, swingDelay)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<Armor> Armor(string unidentName, ArmorName type, int itemRarity) 
            => Entities.CreateBuilder<Armor>()
            .WithName(type.ToString())
            .WithSprite(type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithArmorInfo(type)
            .WithItemInfo(itemRarity, unidentName)
            ;

        private EntityBuilder<T> Consumable<T>(int itemRarity, int remainingUses, int maxUses, bool consumedWhenEmpty, string unidentName = null)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite("None", ColorName.White)
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<Spell> Spell(SpellName type)
            => Entities.CreateBuilder<Spell>()
            .WithName(type.ToString())
            .WithSprite(type.ToString(), ColorName.White)
            .WithSpellInfo(type)
            ;

        public EntityBuilder<Potion> Potion(PotionName type)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + (int)type * 17);

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
               .WithName($"Potion of {type}")
               .WithSprite(nameof(Potion), potionColor.Color)
               .WithPotionInfo(type)
               .WithIntrinsicEffect(() => type switch {
                   PotionName.Healing => new VampirismEffect().Temporary(10).GrantedOnUse(),
                   _ => throw new NotImplementedException()
               });
           ;
        }

        public EntityBuilder<Scroll> Scroll(ScrollName type)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + (int)type * 31);
            var label = ScrollLabel();
            return Consumable<Scroll>(unidentName: $"scroll labelled '{label}'", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
            .WithName($"Scroll of {type}")
            .WithSprite(nameof(Scroll), ColorName.Yellow)
            .WithScrollInfo(type)
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
            .WithSprite(type.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithFeatureInfo(type)
            ;

        private EntityBuilder<Tile> Tile(TileName type, ColorName color)
            => Entities.CreateBuilder<Tile>()
            .WithName(type.ToString())
            .WithSprite(type.ToString(), color)
            .WithPhysics(Coord.Zero)
            .WithTileInfo(type)
            ;

        #region NPCs
        public EntityBuilder<Actor> NPC_Rat() 
            => Enemy()
            .WithName(nameof(ActorName.Rat))
            .WithActorInfo(ActorName.Rat)
            .WithFaction(FactionName.Rats)
            .WithSprite(nameof(ActorName.Rat), ColorName.White)
            .WithBlood(ColorName.LightRed, 25)
            ;

        public EntityBuilder<Actor> NPC_Snake() 
            => Enemy()
            .WithName(nameof(ActorName.Snake))
            .WithActorInfo(ActorName.Snake)
            .WithFaction(FactionName.Snakes)
            .WithSprite(nameof(ActorName.Snake), ColorName.White)
            .WithBlood(ColorName.LightRed, 10)
            ;

        public EntityBuilder<Actor> NPC_Cat() 
            => Enemy()
            .WithName(nameof(ActorName.Cat))
            .WithActorInfo(ActorName.Cat)
            .WithFaction(FactionName.Cats)
            .WithSprite(nameof(ActorName.Cat), ColorName.White)
            .WithBlood(ColorName.LightRed, 50)
            ;

        public EntityBuilder<Actor> NPC_Dog() 
            => Enemy()
            .WithName(nameof(ActorName.Dog))
            .WithActorInfo(ActorName.Dog)
            .WithFaction(FactionName.Dogs)
            .WithSprite(nameof(ActorName.Dog), ColorName.White)
            .WithBlood(ColorName.LightRed, 80)
            ;

        public EntityBuilder<Actor> NPC_Boar() 
            => Enemy()
            .WithName(nameof(ActorName.Boar))
            .WithActorInfo(ActorName.Boar)
            .WithFaction(FactionName.Boars)
            .WithSprite(nameof(ActorName.Boar), ColorName.White)
            .WithBlood(ColorName.LightRed, 200)
            ;
        #endregion

        #region BOSSES
        public EntityBuilder<Actor> Boss_NpcGreatKingRat() 
            => NPC_Rat()
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithDialogueTriggers(NpcName.GreatKingRat)
            .WithSprite(nameof(NpcName.GreatKingRat), ColorName.White)
            ;
        #endregion

        #region WEAPONS
        public EntityBuilder<Weapon> Weapon_Sword()
            => Weapon("sword", WeaponName.Sword, AttackName.Melee, WeaponHandednessName.OneHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Weapon_Bow()
            => Weapon("bow", WeaponName.Bow, AttackName.Ranged, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Weapon_Staff()
            => Weapon("staff", WeaponName.Staff, AttackName.Magical, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        #endregion

        #region ARMORS
        public EntityBuilder<Armor> Armor_Leather()
            => Armor($"leather armor", ArmorName.LeatherArmor, itemRarity: 10)
            ;
        #endregion

        #region SPELLS
        public EntityBuilder<Spell> Spell_Bloodbath()
            => Spell(SpellName.Bloodbath)
            .WithIntrinsicEffect(() => new BloodbathEffect())
            ;
        public EntityBuilder<Spell> Spell_ClotBlock()
            => Spell(SpellName.ClotBlock)
            .WithIntrinsicEffect(() => new ClotBlockEffect())
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
        public EntityBuilder<Tile> Tile_Ground()
            => Tile(TileName.Ground, ColorName.LightGray)
            ;
        public EntityBuilder<Tile> Tile_Unimplemented()
            => Tile(TileName.Error, ColorName.LightMagenta)
            ;
        #endregion
    }
}
