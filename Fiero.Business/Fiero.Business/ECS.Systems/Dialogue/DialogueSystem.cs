using Unconcern.Common;

namespace Fiero.Business
{
    public class DialogueSystem : EcsSystem
    {
        protected readonly GameEntities Entities;
        protected readonly GameDialogues Dialogues;
        protected readonly GameDataStore Store;
        protected readonly GameUI UI;

        public readonly record struct DialogueTriggeredEvent(IDialogueTrigger Trigger, DialogueNode Node, PhysicalEntity Speaker, DrawableEntity[] Listeners);
        public readonly SystemEvent<DialogueSystem, DialogueTriggeredEvent> DialogueTriggered;


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
            DialogueTriggered = new(this, nameof(DialogueTriggered));
        }

        public void CheckTriggers()
        {
            foreach (var comp in Entities.GetComponents<DialogueComponent>())
            {
                var speaker = default(PhysicalEntity);
                var floorId = default(FloorId);
                if (!Entities.TryGetProxy<Actor>(comp.EntityId, out var actorSpeaker))
                {
                    // This is a dialogue that was triggered by a dungeon feature
                    if (!Entities.TryGetProxy<Feature>(comp.EntityId, out var featureSpeaker))
                    {
                        throw new ArgumentException();
                    }
                    floorId = featureSpeaker.Physics.FloorId;
                    speaker = featureSpeaker;
                }
                else
                {
                    floorId = actorSpeaker.FloorId();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers)
                {
                    if (trigger.TryTrigger(floorId, speaker, out var listeners))
                    {
                        var node = Dialogues.GetDialogue(trigger.Node)
                            .Format(trigger.Arguments);
                        trigger.OnTrigger();
                        var list = listeners.ToArray();
                        _ = DialogueTriggered.Raise(new(trigger, node, speaker, list));
                        var modal = UI.Dialogue(trigger, node, speaker, list);
                        modal.NextChoice += (m, node) =>
                        {
                            _ = DialogueTriggered.Raise(new(trigger, node, speaker, list));
                        };
                        modal.Closed += (e, m) =>
                        {
                            // (not cancelled)
                            if (m.ResultType == true && !trigger.Repeatable)
                            {
                                comp.Triggers.Remove(trigger);
                            }
                        };
                        return;
                    }
                }
            }
        }
    }
}
