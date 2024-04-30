using Ergo.Lang;

namespace Fiero.Business
{
    [Term(Marshalling = TermMarshalling.Named)]
    public class Actor : PhysicalEntity
    {
        [NonTerm]
        [RequiredComponent]
        public ActionComponent Action { get; private set; }
        [RequiredComponent]
        [Term(Key = "props", Marshalling = TermMarshalling.Named)]
        public ActorComponent ActorProperties { get; private set; }
        [NonTerm]
        [RequiredComponent]
        public FactionComponent Faction { get; private set; }
        [NonTerm]
        public NpcComponent Npc { get; private set; }
        [NonTerm]
        public LogComponent Log { get; private set; }
        public ActorEquipmentComponent ActorEquipment { get; private set; }
        [NonTerm]
        public DialogueComponent Dialogue { get; private set; }
        [NonTerm]
        public AiComponent Ai { get; private set; }
        [RequiredComponent]
        public FieldOfViewComponent Fov { get; private set; }
        [NonTerm]
        public SpellLibraryComponent Spells { get; private set; }
        [NonTerm]
        public PartyComponent Party { get; private set; }
    }
}
