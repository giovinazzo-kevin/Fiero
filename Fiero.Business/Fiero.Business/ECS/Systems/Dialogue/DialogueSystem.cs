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
        protected readonly GameEntities Entities;
        protected readonly GameDialogues Dialogues;
        protected readonly GameDataStore Store;
        protected readonly GameUI UI;

        public DialogueSystem(
            EventBus bus,
            GameDialogues dialogues,
            GameEntities entities,
            GameDataStore store,
            GameUI ui)
            : base(bus)
        {
            Dialogues = dialogues;
            Entities = entities;
            Store = store;
            UI = ui;
        }

        public void SetTriggers(GameSystems systems, NpcName type, DialogueComponent component)
        {
            switch (type) {
                case NpcName.GreatKingRat:
                    foreach (var t in GreatKingRat()) component.Triggers.Add(t);
                    break;
            }
            IEnumerable<IDialogueTrigger> GreatKingRat()
            {
                yield return new PlayerInSightDialogueTrigger<GKRDialogueName>(
                    systems, GKRDialogueName.JustMet, repeatable: false);
            }
        }

        public void SetTriggers(GameSystems systems, FeatureName type, DialogueComponent component)
        {
            switch (type) {
                case FeatureName.Shrine:
                    foreach (var t in Shrine()) component.Triggers.Add(t);
                    break;
            }
            IEnumerable<IDialogueTrigger> Shrine()
            {
                return Rng.Random.Choose(
                    Smintheus()
                );

                IEnumerable<IDialogueTrigger> Smintheus()
                {
                    yield return new BumpedByPlayerDialogueTrigger<ShrineDialogueName>(
                        systems, ShrineDialogueName.Smintheus, repeatable: true);
                }
            }
        }

        public void CheckTriggers()
        {
            foreach (var comp in Entities.GetComponents<DialogueComponent>()) {
                var dialogueKey = default(string);
                var speaker = default(Drawable);
                var floorId = default(FloorId);
                if (!Entities.TryGetProxy<Actor>(comp.EntityId, out var actorSpeaker)) {
                    // This is a dialogue that was triggered by a dungeon feature
                    if (!Entities.TryGetProxy<Feature>(comp.EntityId, out var featureSpeaker)) {
                        throw new ArgumentException();
                    }
                    floorId = featureSpeaker.FeatureProperties.FloorId;
                    dialogueKey = featureSpeaker.FeatureProperties.Type.ToString();
                    speaker = featureSpeaker;
                }
                else {
                    floorId = actorSpeaker.ActorProperties.FloorId;
                    dialogueKey = actorSpeaker.Npc?.Type.ToString() ?? actorSpeaker.ActorProperties.Type.ToString();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers) {
                    if (trigger.TryTrigger(floorId, speaker, out var listeners)) {
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
