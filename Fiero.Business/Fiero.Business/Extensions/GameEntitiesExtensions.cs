using Fiero.Core;
using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace Fiero.Business
{

    public static class GameEntitiesExtensions
    {
        public static int CreateEntity(this GameEntities entities, string name, string sprite, Coord position, Color? tint = null)
        {
            var entity = entities.CreateEntity();
            entities.AddComponent<RenderComponent>(entity, c => {
                c.SpriteName = sprite;
                c.Sprite.Scale = new(2, 2);
                c.Sprite.Position = new(position.X * c.Sprite.Scale.X, position.Y * c.Sprite.Scale.Y);
                c.Sprite.Color = tint != null ? new(tint.Value.R, tint.Value.G, tint.Value.B, tint.Value.A) : c.Sprite.Color;
                return c;
            });
            entities.AddComponent<PhysicsComponent>(entity, c => {
                c.Position = position;
                return c;
            });
            entities.AddComponent<InfoComponent>(entity, c => {
                c.Name = name;
                return c;
            });
            return entity;
        }

        public static int CreateEnemy(
           this GameEntities entities,
           Coord position,
           ActorName type,
           FactionName faction,
           NpcName? npc = null,
           string name = null,
           string sprite = null,
           Color? tint = null
       )
        {
            var factionSystem = (FactionSystem)entities.ServiceFactory.GetInstance(typeof(FactionSystem));
            var entity = entities.CreateEntity(
                name ?? npc?.ToString() ?? type.ToString(), 
                sprite ?? npc?.ToString() ?? type.ToString(), 
                position, 
                tint
            );
            entities.AddComponent<ActionComponent>(entity, c => {
                (c.Path, c.Target, c.Direction) = (null, null, null);
                c.ActionProvider = ActionProvider.EnemyAI();
                return c;
            });
            entities.AddComponent<FactionComponent>(entity, c => {
                c.Type = faction;
                c.Relationships = factionSystem.GetRelationships(c.Type);
                return c;
            });
            entities.AddComponent<ActorComponent>(entity, c => {
                c.Type = type;
                c.Health = c.MaximumHealth = 5;
                c.Personality = Personality.RandomPersonality();
                return c;
            });
            if (npc.HasValue) {
                entities.AddComponent<DialogueComponent>(entity, d => {
                    DialogueTriggers.Set(npc.Value, d);
                    return d;
                });
                entities.AddComponent<NpcComponent>(entity, c => {
                    c.Type = npc.Value;
                    return c;
                });
            }
            return entity;
        }

        public static int CreatePlayer(this GameEntities entities, GameInput input, string name, string sprite, Coord position, Color? tint = null)
        {
            var factionSystem = (FactionSystem)entities.ServiceFactory.GetInstance(typeof(FactionSystem));
            var entity = entities.CreateEntity(name, sprite, position, tint);
            entities.AddComponent<ActionComponent>(entity, c => {
                (c.Path, c.Target, c.Direction) = (null, null, null);
                c.ActionProvider = ActionProvider.PlayerInput(input);
                return c;
            });
            entities.AddComponent<FactionComponent>(entity, c => {
                c.Type = FactionName.Players;
                c.Relationships = factionSystem.GetRelationships(c.Type);
                return c;
            });
            entities.AddComponent<ActorComponent>(entity, c => {
                c.Type = ActorName.Player;
                c.Health = c.MaximumHealth = 10;
                return c;
            });
            entities.AddComponent<LogComponent>(entity, c => {
                return c;
            });
            return entity;
        }

        public static int CreateItem(this GameEntities entities, ItemName type, Coord position, string name = null, string sprite = null, Color? tint = null)
        {
            var entity = entities.CreateEntity(name ?? type.ToString(), sprite ?? type.ToString(), position, tint);
            entities.AddComponent<ItemComponent>(entity, c => {
                c.Type = type;
                return c;
            });
            return entity;
        }

        public static int CreateFeature(this GameEntities entities, FeatureName type, Coord position, string name = null, string sprite = null, Color? tint = null)
        {
            var entity = entities.CreateEntity(name ?? type.ToString(), sprite ?? type.ToString(), position, tint);
            entities.AddComponent<FeatureComponent>(entity, c => {
                c.Type = type;
                c.BlocksMovement = c.Type switch {
                    FeatureName.Shrine => true,
                    FeatureName.Chest => true,
                    _ => false
                };
                return c;
            });
            entities.AddComponent<DialogueComponent>(entity, d => {
                DialogueTriggers.Set(type, d);
                return d;
            });
            return entity;
        }

        public static int CreateTile(this GameEntities entities, TileName tile, Coord position)
        {
            // TODO: tile size??
            var entity = CreateEntity(entities, tile.ToString(), tile.ToString().ToLowerInvariant(), position,
                tint: tile switch {
                    TileName.WallNormal => Color.Gray,
                    TileName.WallStart => Color.Green,
                    TileName.WallItem => Color.Gold,
                    TileName.WallShop => Color.SaddleBrown,
                    TileName.WallBoss => Color.DarkRed,
                    TileName.WallSecret => Color.Purple,
                    TileName.WallEnemies => Color.White,
                    TileName.Door => Color.SaddleBrown,
                    TileName.DoorKey => Color.Gold,
                    _ => Color.White,
                });
            entities.AddComponent<TileComponent>(entity, c => {
                c.Name = tile;
                c.BlocksMovement = tile switch {
                    TileName.Ground => false,
                    TileName.Downstairs => false,
                    TileName.Upstairs => false,
                    //TileName.Door => false,
                    //TileName.Door_Key => false,
                    _ => true
                };
                return c;
            });
            return entity;
        }
    }
}
