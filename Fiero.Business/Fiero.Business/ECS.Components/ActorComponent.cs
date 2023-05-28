using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    public class ActorComponent : EcsComponent
    {
        public ActorName Type { get; set; }
        public RaceName Race { get; set; }
        [NonTerm]
        public Stat Level { get; set; }
        public int LVL { get => Level?.V ?? 0; set { if (Level != null) Level.V = value; } }
        public int MaxLVL { get => Level?.Max ?? 0; set { if (Level != null) Level.Max = value; } }
        [NonTerm]
        public Stat Health { get; set; }
        public int HP { get => Health?.V ?? 0; set { if (Health != null) Health.V = value; } }
        public int MaxHP { get => Health?.Max ?? 0; set { if (Health != null) Health.Max = value; } }
        [NonTerm]
        public Stat Magic { get; set; }
        public int MP { get => Magic?.V ?? 0; set { if (Magic != null) Magic.V = value; } }
        public int MaxMP { get => Magic?.Max ?? 0; set { if (Magic != null) Magic.Max = value; } }
        [NonTerm]
        public Stat Experience { get; set; }
        public int XP { get => Experience?.V ?? 0; set { if (Experience != null) Experience.V = value; } }
        public int MaxXP { get => Experience?.Max ?? 0; set { if (Experience != null) Experience.Max = value; } }
        public CorpseDef Corpse { get; set; }
    }
}
