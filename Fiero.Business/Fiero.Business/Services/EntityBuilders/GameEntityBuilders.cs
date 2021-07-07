using Fiero.Core;
using System;
using System.Linq;
using System.Text;

namespace Fiero.Business
{
    public class GameEntityBuilders
    {
        protected readonly GameEntities Entities;
        protected readonly GameInput Input;
        protected readonly GameGlossaries Glossaries;
        protected readonly GameDataStore Store;
        protected readonly GameColors<ColorName> Colors;

        public GameEntityBuilders(
            GameEntities entities, 
            GameInput input,
            GameGlossaries glossaries,
            GameDataStore store,
            GameColors<ColorName> colors
        ) {
            Entities = entities;
            Input = input;
            Glossaries = glossaries;
            Store = store;
            Colors = colors;
        }

        public EntityBuilder<Actor> Player
            => Entities.CreateBuilder<Actor>()
            .WithLogging()
            .WithPlayerAI(Input)
            .WithName(nameof(Player))
            .WithSprite(nameof(Player))
            .WithActorInfo(ActorName.Player, MonsterTierName.One)
            .WithFaction(FactionName.Players)
            .WithPersonality(default)
            .WithPosition(Coord.Zero)
            .WithInventory(50)
            ;

        public EntityBuilder<Actor> Enemy(MonsterTierName tier) 
            => Entities.CreateBuilder<Actor>()
            .WithLogging()
            .WithEnemyAI()
            .WithName(nameof(Enemy))
            .WithSprite("None")
            .WithActorInfo(ActorName.None, tier)
            .WithFaction(FactionName.None)
            .WithPersonality(Personality.RandomPersonality())
            .WithPosition(Coord.Zero)
            .WithInventory(5)
            ;

        public EntityBuilder<Weapon> Weapon(string unidentName, WeaponName type, WeaponHandednessName hands, int baseDamage, int swingDelay, int itemRarity)
            => Entities.CreateBuilder<Weapon>()
            .WithName(type.ToString())
            .WithSprite(type.ToString())
            .WithPosition(Coord.Zero)
            .WithWeaponInfo(type, hands, baseDamage, swingDelay)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<Armor> Armor(string unidentName, ArmorName type, ArmorSlotName slot, int itemRarity) 
            => Entities.CreateBuilder<Armor>()
            .WithName(type.ToString())
            .WithSprite(slot.ToString())
            .WithPosition(Coord.Zero)
            .WithArmorInfo(type, slot)
            .WithItemInfo(itemRarity, unidentName)
            ;

        public EntityBuilder<T> Consumable<T>(string unidentName, int itemRarity, int remainingUses, int maxUses, bool consumedWhenEmpty)
            where T : Consumable
            => Entities.CreateBuilder<T>()
            .WithPosition(Coord.Zero)
            .WithName(nameof(Consumable))
            .WithSprite("None")
            .WithConsumableInfo(remainingUses, maxUses, consumedWhenEmpty)
            .WithItemInfo(itemRarity, unidentName)
            ;
        
        public EntityBuilder<Potion> Potion(EffectName effect)
        {
            var rng = Rng.Seeded(Store.Get(Data.Global.RngSeed) + (int)effect * 17);
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
           .WithName($"Potion of {effect}")
           .WithSprite(nameof(Potion), Colors.Get(potionColor.Color))
           .WithPotionInfo(effect)
           ;
        }

        public EntityBuilder<Scroll> Scroll(EffectName effect)
        {
            var rng = Rng.Seeded(Store.Get(Data.Global.RngSeed) + (int)effect * 31);
            var label = ScrollLabel();
            return Consumable<Scroll>($"scroll labelled '{label}'", itemRarity: 1, remainingUses: 1, maxUses: 1, consumedWhenEmpty: true)
            .WithName($"Scroll of {effect}")
            .WithSprite(nameof(Scroll), Colors.Get(ColorName.Yellow))
            .WithScrollInfo(effect)
            ;

            string ScrollLabel()
            {
                var syllables = new[] {
                    "BAH", "DAT", "WAX",
                    "VEL", "ZES", "TEM",
                    "FIR", "NIX", "JIN",
                    "ROS", "VOK", "KOR",
                    "TUN", "QUD", "PUS"
                };
                syllables = syllables.Concat(syllables.Select(x => String.Join("", x.Reverse()))).ToArray();
                return String.Join("", Enumerable.Range(0, 3).Select(x => rng.Choose(syllables)));
            }
        }

        public EntityBuilder<Feature> Feature(FeatureName type)
            => Entities.CreateBuilder<Feature>()
            .WithName(type.ToString())
            .WithSprite(type.ToString())
            .WithPosition(Coord.Zero)
            .WithFeatureInfo(type)
            ;

        public EntityBuilder<Tile> Tile(TileName type)
            => Entities.CreateBuilder<Tile>()
            .WithName(type.ToString())
            .WithSprite(type.ToString(), tint: Colors.Get(type switch {
                TileName.Door       => ColorName.Red,
                TileName.Wall       => ColorName.Gray,
                TileName.Upstairs   => ColorName.LightGray,
                TileName.Downstairs => ColorName.LightGray,
                _ => ColorName.White
            }))
            .WithPosition(Coord.Zero)
            .WithTileInfo(type)
            ;

        #region ENEMIES
        public EntityBuilder<Actor> Rat(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Rats, tier))
            .WithActorInfo(ActorName.Rat, tier)
            .WithFaction(FactionName.Rats)
            .WithSprite(nameof(ActorName.Rat))
            ;

        public EntityBuilder<Actor> Snake(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Snakes, tier))
            .WithActorInfo(ActorName.Snake, tier)
            .WithFaction(FactionName.Snakes)
            .WithSprite(nameof(ActorName.Snake))
            ;

        public EntityBuilder<Actor> Cat(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Cats, tier))
            .WithActorInfo(ActorName.Cat, tier)
            .WithFaction(FactionName.Cats)
            .WithSprite(nameof(ActorName.Cat))
            ;

        public EntityBuilder<Actor> Dog(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Dogs, tier))
            .WithActorInfo(ActorName.Dog, tier)
            .WithFaction(FactionName.Dogs)
            .WithSprite(nameof(ActorName.Dog))
            ;

        public EntityBuilder<Actor> Boar(MonsterTierName tier) 
            => Enemy(tier)
            .WithName(Glossaries.GetMonsterName(FactionName.Boars, tier))
            .WithActorInfo(ActorName.Boar, tier)
            .WithFaction(FactionName.Boars)
            .WithSprite(nameof(ActorName.Boar))
            ;
        #endregion

        #region BOSSES
        public EntityBuilder<Actor> NpcGreatKingRat() 
            => Rat(MonsterTierName.Five)
            .WithName("Great King Rat")
            .WithNpcInfo(NpcName.GreatKingRat)
            .WithDialogueTriggers(NpcName.GreatKingRat)
            .WithSprite(nameof(NpcName.GreatKingRat))
            ;
        #endregion

        #region WEAPONS
        public EntityBuilder<Weapon> Sword()
            => Weapon("sword", WeaponName.Sword, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Bow()
            => Weapon("bow", WeaponName.Bow, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        public EntityBuilder<Weapon> Staff()
            => Weapon("staff", WeaponName.Staff, WeaponHandednessName.TwoHanded, 10, 100, itemRarity: 10)
            ;
        #endregion

        #region ARMORS
        public EntityBuilder<Armor> LeatherArmor(ArmorSlotName slot)
            => Armor("leather armor", ArmorName.Light, slot, itemRarity: 10)
            ;
        #endregion

        #region FEATURES
        public EntityBuilder<Feature> Chest()
            => Feature(FeatureName.Chest)
            ;
        public EntityBuilder<Feature> Shrine()
            => Feature(FeatureName.Shrine)
            .WithDialogueTriggers(FeatureName.Shrine)
            ;
        public EntityBuilder<Feature> Trap()
            => Feature(FeatureName.Trap)
            ;
        #endregion
    }
}
