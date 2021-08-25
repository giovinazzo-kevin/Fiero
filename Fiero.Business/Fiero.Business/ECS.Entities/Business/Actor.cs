using Fiero.Core;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Fiero.Business
{
    public class Actor : PhysicalEntity
    {
        [RequiredComponent]
        public ActionComponent Action { get; private set; }
        [RequiredComponent]
        public ActorComponent ActorProperties { get; private set; }
        [RequiredComponent]
        public FactionComponent Faction { get; private set; }
        public NpcComponent Npc { get; private set; }
        public LogComponent Log { get; private set; }
        public EquipmentComponent Equipment { get; private set; }
        public DialogueComponent Dialogue { get; private set; }
        public AiComponent Ai { get; private set; }
        public FieldOfViewComponent Fov { get; private set; }
        public SpellLibraryComponent Spells { get; private set; }
    }
}
