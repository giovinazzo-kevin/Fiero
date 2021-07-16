using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using Unconcern.Common;
using static SFML.Window.Keyboard;

namespace Fiero.Business.Scenes
{
    public class GameplayScene : GameScene<GameplayScene.SceneState>
    {
        public enum SceneState
        {
            [EntryState]
            Main,
            [ExitState]
            Exit_GameOver,
            [ExitState]
            Exit_SaveAndQuit
        }

        protected readonly GameDataStore Store;
        protected readonly GameEntities Entities;
        protected readonly GameInput Input;

        protected readonly FloorSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly ActionSystem ActionSystem;
        protected readonly RenderSystem RenderSystem;
        protected readonly DialogueSystem DialogueSystem;
        protected readonly GameDialogues Dialogues;
        protected readonly GameSounds<SoundName> Sounds;
        protected readonly GameEntityBuilders EntityBuilders;
        protected readonly OffButton OffButton;
        protected readonly GameUI UI;

        public Actor Player { get; private set; }

        public GameplayScene(
            GameInput input,
            GameDataStore store,
            GameEntities entities,
            GameDialogues dialogues,
            FloorSystem floorSystem,
            RenderSystem renderSystem,
            DialogueSystem dialogueSystem,
            FactionSystem factionSystem,
            ActionSystem actionSystem,
            GameEntityBuilders entityBuilders,
            GameUI ui,
            OffButton off,
            GameSounds<SoundName> sounds)
        {
            Input = input;
            Store = store;
            Entities = entities;
            FloorSystem = floorSystem;
            RenderSystem = renderSystem;
            ActionSystem = actionSystem;
            FactionSystem = factionSystem;
            DialogueSystem = dialogueSystem;
            Dialogues = dialogues;
            EntityBuilders = entityBuilders;
            UI = ui;
            OffButton = off;
            Sounds = sounds;
        }

        public override async Task InitializeAsync()
        {
            RenderSystem.Initialize();
            SubscribeDialogueHandlers();
            await base.InitializeAsync();
        }

        public override IEnumerable<Subscription> RouteEvents()
        {
            yield return ActionSystem.ActorTurnEnded.SubscribeHandler(msg => {
                    if(msg.Content.Message.Actor.Id == Player.Id) {
                        DialogueSystem.CheckTriggers();
                    }
                });
        }

        public bool TrySpawn(int entityId, out Actor actor, float maxDistance = 10)
        {
            actor = FloorSystem.CurrentFloor.AddActor(entityId);
            if(!FloorSystem.TryGetClosestFreeTile(actor.Physics.Position, out var spawnTile, maxDistance)) {
                return false;
            }
            actor.Physics.Position = spawnTile.Physics.Position;
            ActionSystem.AddActor(entityId);
            return true;
        }
        
        protected void SubscribeDialogueHandlers()
        {
            Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet)
                .Triggered += (t, eh) => {
                    Sounds.Get(SoundName.BossSpotted).Play();
                };
            Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet_Friend)
                .Triggered += (t, eh) => {
                    foreach (var player in eh.DialogueListeners.Players()) {
                        FactionSystem.TryUpdateRelationship(FactionName.Rats, FactionName.Players, 
                            x => x.With(StandingName.Loved), out _);
                    }
                };
            Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet_Enemy)
                .Triggered += (t, eh) => {
                    foreach (var player in eh.DialogueListeners.Players()) {
                        FactionSystem.TryUpdateRelationship(FactionName.Rats, FactionName.Players,
                            x => x.With(StandingName.Hated), out _);
                        FactionSystem.TryCreateConflict(
                            FactionName.Rats, (r, i) => i < 3,
                            FactionName.Players, (p, i) => i == 0,
                            out _);
                        FactionSystem.TryCreateConflict(
                            FactionName.Rats, (r, i) => i < 3,
                            FactionName.Players, (p, i) => i == 0,
                            out _);
                        FactionSystem.TryCreateConflict(
                            FactionName.Rats, (r, i) => i < 3,
                            FactionName.Players, (p, i) => i == 0,
                            out _);
                        FactionSystem.TryCreateConflict(
                            FactionName.Rats, (r, i) => i < 3,
                            FactionName.Players, (p, i) => i == 0,
                            out _);
                    }
                };
            Dialogues.GetDialogue(FeatureName.Shrine, ShrineDialogueName.Smintheus_Follow)
                .Triggered += (t, eh) => {
                    foreach (var player in eh.DialogueListeners.Players()) {
                        var friends = Enumerable.Range(0, 5)
                            .Select(i => EntityBuilders
                                .Rat(MonsterTierName.Two)
                                .WithFaction(FactionName.Players)
                                .WithPosition(player.Physics.Position)
                                .Build());
                        foreach (var f in friends) {
                            TrySpawn(f.Id, out _);
                        }
                    }
                    // Remove trigger from the shrine
                    if(Entities.TryGetFirstComponent<DialogueComponent>(eh.DialogueStarter.Id, out var dialogue)) {
                        dialogue.Triggers.Remove(t);
                    }
                };
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            RenderSystem.Update(win, t, dt);
            ActionSystem.Update();
            Entities.RemoveFlaggedItems();
            if (Input.IsKeyPressed(Key.R)) {
                TrySetState(SceneState.Main);
            }
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Clear();
            RenderSystem.Draw(win, t, dt);
        }

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            if(State == SceneState.Main) {
                var newRngSeed = (int)DateTime.Now.ToBinary();
                Store.SetValue(Data.Global.RngSeed, newRngSeed);
                Entities.Clear();
                FloorSystem.Clear();
                ActionSystem.Clear();
                // Generate map
                FloorSystem.AddFloor(new (100, 100), floor =>
                    floor.WithStep(ctx => {
                        var dungeon = new DungeonGenerator(DungeonGenerationSettings.Default)
                            .Generate();
                        ctx.DrawBox((0, 0), (ctx.Size.X, ctx.Size.Y), TileName.Wall);
                        ctx.DrawDungeon(dungeon);
                    }));
                // Track agents
                foreach (var comp in Entities.GetComponents<ActionComponent>()) {
                    ActionSystem.AddActor(comp.EntityId);
                }
                // Create player on top of the starting stairs
                var playerName = Store.GetOrDefault(Data.Player.Name, "Player");
                var upstairs = FloorSystem.CurrentFloor.Tiles.Values
                    .Single(t => t.TileProperties.Name == TileName.Upstairs)
                    .Physics.Position;
                var pid = EntityBuilders.Player
                    .WithPosition(upstairs)
                    .WithName(playerName)
                    .Build().Id;
                if(!TrySpawn(pid, out var player)) {
                    throw new InvalidOperationException("Can't spawn the player??");
                }
                RenderSystem.SelectedActor.Following.V = Player = player;
            }
            base.OnStateChanged(oldState);
        }
    }
}
