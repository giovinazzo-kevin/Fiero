﻿using Fiero.Core;
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
            .WithLogging()
            .WithPlayerAi(UI)
            .WithName(nameof(Player))
            .WithSprite(nameof(Player), ColorName.White)
            .WithActorInfo(ActorName.Player, MonsterTierName.One)
            .WithFaction(FactionName.Players)
            .WithPhysics(Coord.Zero)
            .WithInventory(50)
            .WithEquipment()
            .WithFieldOfView(7)
            ;

        private EntityBuilder<Actor> Enemy(MonsterTierName tier) 
            => Entities.CreateBuilder<Actor>()
            .WithLogging()
            .WithEnemyAi()
            .WithName(nameof(Enemy))
            .WithSprite("None", ColorName.White)
            .WithActorInfo(ActorName.None, tier)
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

        public EntityBuilder<Armor> Armor(string unidentName, ArmorName type, ArmorSlotName slot, int itemRarity) 
            => Entities.CreateBuilder<Armor>()
            .WithName(type.ToString())
            .WithSprite(slot.ToString(), ColorName.White)
            .WithPhysics(Coord.Zero)
            .WithArmorInfo(type, slot)
            .WithItemInfo(itemRarity, unidentName)
            ;

        private EntityBuilder<T> Consumable<T>(string unidentName, int itemRarity, int remainingUses, int maxUses, bool consumedWhenEmpty)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPhysics(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite("None", ColorName.White)
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<Potion> Potion(PotionEffectName effectType)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + (int)effectType * 17);
            var potionColor = rng.Choose(new(ColorName Color, string Adjective)[] {
                (ColorName.White, "pale"),
                (ColorName.Red, "warm"),
                (ColorName.Green, "slimy"),
                (ColorName.Blue, "aqueous"),
                (ColorName.Cyan, "clear"),
                (ColorName.Yellow, "foaming"),
                (ColorName.Magenta, "fizzling"),
                (ColorName.Gray, "murky"),
                (ColorName.LightGray, "sedimented"),
                (ColorName.LightRed, "glittering"),
                (ColorName.LightGreen, "glowing"),
                (ColorName.LightBlue, "cold"),
                (ColorName.LightCyan, "gelatinous"),
                (ColorName.LightYellow, "bubbling"),
                (ColorName.LightMagenta, "lumpy"),
                (ColorName.Black, "dark")
            });

            return Consumable<Potion>($"{potionColor.Adjective} potion", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
               .WithName($"Potion of {effectType}")
               .WithSprite(nameof(Potion), potionColor.Color)
               .WithPotionInfo(effectType)
               .WithIntrinsicEffect(() => effectType switch {
                   PotionEffectName.Healing => new VampirismEffect().Temporary(10).GrantedOnUse(),
                   _ => throw new NotImplementedException()
               });
           ;
        }

        public EntityBuilder<Scroll> Scroll(ScrollEffectName effect)
        {
            var rng = Rng.Seeded(UI.Store.Get(Data.Global.RngSeed) + (int)effect * 31);
            var label = ScrollLabel();
            return Consumable<Scroll>($"scroll labelled '{label}'", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
            .WithName($"Scroll of {effect}")
            .WithSprite(nameof(Scroll), ColorName.Yellow)
            .WithScrollInfo(effect)
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

        private EntityBuilder<Feature> Feature(FeatureName type)
            => Entities.CreateBuilder<Feature>()
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

        #region ENEMIES
        public EntityBuilder<Actor> Rat(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Rats, tier))
            .WithActorInfo(ActorName.Rat, tier)
            .WithFaction(FactionName.Rats)
            .WithSprite(nameof(ActorName.Rat), ColorName.White)
            ;

        public EntityBuilder<Actor> Snake(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Snakes, tier))
            .WithActorInfo(ActorName.Snake, tier)
            .WithFaction(FactionName.Snakes)
            .WithSprite(nameof(ActorName.Snake), ColorName.White)
            ;

        public EntityBuilder<Actor> Cat(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Cats, tier))
            .WithActorInfo(ActorName.Cat, tier)
            .WithFaction(FactionName.Cats)
            .WithSprite(nameof(ActorName.Cat), ColorName.White)
            ;

        public EntityBuilder<Actor> Dog(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Dogs, tier))
            .WithActorInfo(ActorName.Dog, tier)
            .WithFaction(FactionName.Dogs)
            .WithSprite(nameof(ActorName.Dog), ColorName.White)
            ;

        public EntityBuilder<Actor> Boar(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Boars, tier))
            .WithActorInfo(ActorName.Boar, tier)
            .WithFaction(FactionName.Boars)
            .WithSprite(nameof(ActorName.Boar), ColorName.White)
            ;
        #endregion

        #region BOSSES
        public EntityBuilder<Actor> NpcGreatKingRat() 
            => Rat(MonsterTierName.Five)
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithDialogueTriggers(NpcName.GreatKingRat)
            .WithSprite(nameof(NpcName.GreatKingRat), ColorName.White)
            ;
        #endregion

        #region WEAPONS
        public EntityBuilder<Weapon> Sword()
            => Weapon("sword", WeaponName.Sword, AttackName.Melee, WeaponHandednessName.OneHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Bow()
            => Weapon("bow", WeaponName.Bow, AttackName.Ranged, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Staff()
            => Weapon("staff", WeaponName.Staff, AttackName.Magical, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        #endregion

        #region ARMORS
        public EntityBuilder<Armor> LeatherArmor(ArmorSlotName slot)
            => Armor($"leather armor ({slot})", ArmorName.Light, slot, itemRarity: 10)
            ;
        #endregion

        #region FEATURES
        private ColorName GetBranchColor(DungeonBranchName branch) => branch switch {
            DungeonBranchName.Dungeon => ColorName.Gray,
            DungeonBranchName.Sewers => ColorName.Green,
            DungeonBranchName.Kennels => ColorName.Red,
            _ => ColorName.White
        };

        public EntityBuilder<Feature> Chest()
            => Feature(FeatureName.Chest)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = true)
            ;
        public EntityBuilder<Feature> Shrine()
            => Feature(FeatureName.Shrine)
            .WithDialogueTriggers(FeatureName.Shrine)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = true)
            ;
        public EntityBuilder<Feature> Trap()
            => Feature(FeatureName.Trap)
            .WithIntrinsicEffect(() => new TrapEffect())
            .Tweak<RenderComponent>(x => x.Hidden = true)
            ;
        public EntityBuilder<Feature> Door()
            => Feature(FeatureName.Door)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = x.BlocksLight = true)
            ;
        public EntityBuilder<Feature> Downstairs(FloorConnection conn)
            => Feature(FeatureName.Downstairs)
            .WithColor(GetBranchColor(conn.To.Branch))
            .WithPortalInfo(conn)
            ;
        public EntityBuilder<Feature> Upstairs(FloorConnection conn)
            => Feature(FeatureName.Upstairs)
            .WithColor(GetBranchColor(conn.From.Branch))
            .WithPortalInfo(conn)
            ;
        public EntityBuilder<Feature> BloodSplatter(ColorName color = ColorName.Red)
            => Feature(FeatureName.BloodSplatter)
            .WithColor(color)
            ;
        #endregion

        #region TILES
        public EntityBuilder<Tile> WallTile()
            => Tile(TileName.Wall, ColorName.Gray)
            .Tweak<PhysicsComponent>(x => x.BlocksMovement = x.BlocksLight = true)
            ;
        public EntityBuilder<Tile> GroundTile()
            => Tile(TileName.Ground, ColorName.LightGray)
            ;
        public EntityBuilder<Tile> UnimplementedTile()
            => Tile(TileName.Error, ColorName.LightMagenta)
            ;
        #endregion
    }
}
