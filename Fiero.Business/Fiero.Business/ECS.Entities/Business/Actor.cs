using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    public class Actor : PhysicalEntity
    {
        [NonTerm]
        [RequiredComponent]
        public ActionComponent Action { get; private set; }
        // [NonTerm]
        [RequiredComponent]
        public ActorComponent ActorProperties { get; private set; }
        [NonTerm]
        [RequiredComponent]
        public FactionComponent Faction { get; private set; }
        [NonTerm]
        public NpcComponent Npc { get; private set; }
        [NonTerm]
        public LogComponent Log { get; private set; }
        public EquipmentComponent Equipment { get; private set; }
        [NonTerm]
        public DialogueComponent Dialogue { get; private set; }
        [NonTerm]
        public AiComponent Ai { get; private set; }
        [NonTerm]
        public FieldOfViewComponent Fov { get; private set; }
        [NonTerm]
        public SpellLibraryComponent Spells { get; private set; }
        [NonTerm]
        public PartyComponent Party { get; private set; }
    }
}
