using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class DialogueSystem
    {
        protected readonly FloorSystem FloorSystem;
        protected readonly GameEntities Entities;
        protected readonly GameGlossaries Glossaries;
        protected readonly GameDialogues Dialogues;
        protected readonly GameDataStore Store;
        protected readonly GameUI<FontName, TextureName, SoundName> UI;
        protected readonly GameSprites<TextureName> Sprites;

        public DialogueNode CurrentDialogue => UIDialogue.Node;
        protected IDialogueTrigger CurrentTrigger { get; set; }
        protected Drawable CurrentSpeaker { get; set; }
        protected Drawable[] CurrentListeners { get; set; }



        protected Layout UILayout { get; private set; }
        protected ActorDialogue UIDialogue { get; private set; }


        public DialogueSystem(
            FloorSystem floorSystem,
            GameGlossaries glossaries,
            GameDialogues dialogues,
            GameEntities entities,
            GameDataStore store,
            GameUI<FontName, TextureName, SoundName> ui,
            GameSprites<TextureName> sprites)
        {
            FloorSystem = floorSystem;
            Glossaries = glossaries;
            Dialogues = dialogues;
            Entities = entities;
            Store = store;
            UI = ui;
            Sprites = sprites;
        }

        public void Initialize()
        {
            var tileSize = Store.GetOrDefault(Data.UI.TileSize, 8);
            UILayout = UI.CreateLayout()
                .WithFont(FontName.UI)
                .WithTexture(TextureName.UI)
                .WithTileSize(tileSize)
                .ActorDialogue(new(1, 1), new(98, 10), initialize: x => UIDialogue = x)
                .Build();
            UIDialogue.NodeChanged += node => node.Trigger(CurrentTrigger, CurrentSpeaker, CurrentListeners);

        }

        public void Update(float t, float dt)
        {
            if (CurrentDialogue == null)
                return;
            UILayout.Update(t, dt);
        }

        public void Draw(RenderWindow win, float t, float dt)
        {
            if (CurrentDialogue == null)
                return;
            win.Draw(UILayout);
        }

        public void OnPlayerTurnStarted()
        {
            if (CurrentDialogue != null) {
                throw new InvalidOperationException("Why is the turn counter advancing during a dialogue??");
            }
            foreach (var comp in Entities.GetComponents<DialogueComponent>()) {
                var dialogueKey = default(string);
                var speaker = default(Drawable);
                if (!Entities.TryGetProxy<Actor>(comp.EntityId, out var actorSpeaker)) {
                    // This is a dialogue that was triggered by a dungeon feature
                    if (!Entities.TryGetProxy<Feature>(comp.EntityId, out var featureSpeaker)) {
                        throw new ArgumentException();
                    }
                    dialogueKey = featureSpeaker.Properties.Type.ToString();
                    speaker = featureSpeaker;
                }
                else {
                    dialogueKey = actorSpeaker.Properties.Type.ToString();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers) {
                    if (trigger.TryTrigger(FloorSystem.CurrentFloor, speaker, out var listeners)) {
                        var node = Dialogues.GetDialogue(dialogueKey, trigger.DialogueNode);
                        if (node == null) {
                            throw new ArgumentException(trigger.DialogueNode);
                        }
                        CurrentTrigger = trigger;
                        CurrentSpeaker = speaker;
                        CurrentListeners = listeners.ToArray();
                        UIDialogue.Node = node; // triggers the dialogue handlers!
                        if (!trigger.Repeatable) {
                            comp.Triggers.Remove(trigger);
                        }
                        return;
                    }
                }
            }
        }
    }
}
