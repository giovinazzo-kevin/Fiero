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
        protected readonly GameUI UI;
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
            GameUI ui,
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
                .Build(new(), grid => grid
                    .Row(h: 0.25f)
                        .Cell<ActorDialogue>(d => UIDialogue = d)
                    .End());
            UIDialogue.Node.ValueChanged += (owner, old) => UIDialogue.Node.V?.Trigger(CurrentTrigger, CurrentSpeaker, CurrentListeners);
            Data.UI.WindowSize.ValueChanged += e => {
                UILayout.Size.V = e.NewValue.Clamp(0, 800, 0, 800);
                UILayout.Position.V = new(e.NewValue.X / 2 - UILayout.Size.V.X / 2, 0);
            };
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
                    dialogueKey = actorSpeaker.Npc?.Type.ToString() ?? actorSpeaker.Properties.Type.ToString();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers) {
                    if (trigger.TryTrigger(FloorSystem.CurrentFloor, speaker, out var listeners)) {
                        var node = Dialogues.GetDialogue(dialogueKey, trigger.DialogueNode);
                        CurrentTrigger = trigger;
                        CurrentSpeaker = speaker;
                        CurrentListeners = listeners.ToArray();
                        UIDialogue.Node.V = node ?? throw new ArgumentException(trigger.DialogueNode); // triggers the dialogue handlers!
                        if (!trigger.Repeatable) {
                            comp.Triggers.Remove(trigger);
                        }
                        trigger.OnTrigger();
                        return;
                    }
                }
            }
        }
    }
}
