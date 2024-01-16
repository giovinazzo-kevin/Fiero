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

        // TODO: make triggers data driven!!
        public void SetTriggers(MetaSystem systems, NpcName type, DialogueComponent component)
        {
            switch (type)
            {
                case NpcName.GreatKingRat:
                    foreach (var t in GreatKingRat()) component.Triggers.Add(t);
                    break;
            }

            IEnumerable<IDialogueTrigger> GreatKingRat()
            {
                yield return new PlayerInSightDialogueTrigger<GKRDialogueName>(
                    systems, repeatable: false, nameof(NpcName.GreatKingRat), GKRDialogueName.JustMet);
            }
        }

        // TODO: make triggers data driven!!
        public void SetTriggers(MetaSystem systems, FeatureName type, DialogueComponent component)
        {
            switch (type)
            {
                case FeatureName.Shrine:
                    foreach (var t in Shrine()) component.Triggers.Add(t);
                    break;
            }
            IEnumerable<IDialogueTrigger> Shrine()
            {
                return Rng.Random.Choose(new[] {
                    Smintheus()
                });

                IEnumerable<IDialogueTrigger> Smintheus()
                {
                    yield return new BumpedByPlayerDialogueTrigger<ShrineDialogueName>(
                        systems, repeatable: true, nameof(FeatureName.Shrine), ShrineDialogueName.Smintheus);
                }
            }
        }

        public void CheckTriggers()
        {
            foreach (var comp in Entities.GetComponents<DialogueComponent>())
            {
                var dialogueKey = default(string);
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
                    dialogueKey = featureSpeaker.FeatureProperties.Name.ToString();
                    speaker = featureSpeaker;
                }
                else
                {
                    floorId = actorSpeaker.FloorId();
                    dialogueKey = actorSpeaker.Npc?.Type.ToString() ?? actorSpeaker.ActorProperties.Type.ToString();
                    speaker = actorSpeaker;
                }
                foreach (var trigger in comp.Triggers)
                {
                    if (trigger.TryTrigger(floorId, speaker, out var listeners))
                    {
                        var node = Dialogues.GetDialogue(dialogueKey, trigger.Node);
                        if (!trigger.Repeatable)
                        {
                            comp.Triggers.Remove(trigger);
                        }
                        trigger.OnTrigger();
                        var list = listeners.ToArray();
                        _ = DialogueTriggered.Raise(new(trigger, node, speaker, list));
                        UI.Dialogue(trigger, node, speaker, list);
                        return;
                    }
                }
            }
        }
    }
}
