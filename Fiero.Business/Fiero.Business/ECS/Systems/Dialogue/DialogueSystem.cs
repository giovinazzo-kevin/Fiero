using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class DialogueSystem : EcsSystem
    {
        protected readonly FloorSystem FloorSystem;
        protected readonly GameEntities Entities;
        protected readonly GameGlossaries Glossaries;
        protected readonly GameDialogues Dialogues;
        protected readonly GameDataStore Store;
        protected readonly GameUI UI;
        protected readonly GameSprites<TextureName> Sprites;

        public DialogueSystem(
            EventBus bus,
            FloorSystem floorSystem,
            GameGlossaries glossaries,
            GameDialogues dialogues,
            GameEntities entities,
            GameDataStore store,
            GameUI ui,
            GameSprites<TextureName> sprites)
            : base(bus)
        {
            FloorSystem = floorSystem;
            Glossaries = glossaries;
            Dialogues = dialogues;
            Entities = entities;
            Store = store;
            UI = ui;
            Sprites = sprites;
        }

        public void CheckTriggers()
        {
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
                    dialogueKey = actorSpeaker.Npc?.Type.ToString() ?? actorSpeaker.ActorProperties.Type.ToString();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers) {
                    if (trigger.TryTrigger(FloorSystem.CurrentFloor, speaker, out var listeners)) {
                        var node = Dialogues.GetDialogue(dialogueKey, trigger.DialogueNode);
                        if (!trigger.Repeatable) {
                            comp.Triggers.Remove(trigger);
                        }
                        trigger.OnTrigger();
                        UI.Dialogue(trigger, node, speaker, listeners.ToArray());
                        return;
                    }
                }
            }
        }
    }
}
