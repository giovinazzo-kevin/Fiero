using Ergo.Lang;

namespace Fiero.Business
{
    /*
     Experience = The unit's xp. Upon reaching its max value, triggers a level up.
     Level = The unit's level. Upon leveling up, grants an increase to all other stats.
     Health = The unit's health reserves.
     Magic = The unit's magic reserves.
     */

    public class ActorComponent : EcsComponent
    {
        public ActorName Type { get; set; }
        public RaceName Race { get; set; }
        [NonTerm]
        public Stat Experience { get; set; }
        public int XP { get => Experience?.V ?? 0; set { if (Experience != null) Experience.V = value; } }
        public int MaxXP { get => Experience?.Max ?? 0; set { if (Experience != null) Experience.Max = value; } }
        /// <summary>
        /// Determines the steepness of the experience curve.
        /// </summary>
        public float XPExponent { get; set; } = 0.5f;
        /// <summary>
        /// Determines the XP needed to reach level 2.
        /// </summary>
        public int BaseXP { get; set; } = 20;

        [NonTerm]
        public Stat Level { get; set; }
        public int LVL { get => Level?.V ?? 0; set { if (Level != null) Level.V = value; } }
        public int MaxLVL { get => Level?.Max ?? 0; set { if (Level != null) Level.Max = value; } }
        [NonTerm]
        public Stat Health { get; set; }
        public int HP { get => Health?.V ?? 0; set { if (Health != null) Health.V = value; } }
        public int MaxHP { get => Health?.Max ?? 0; set { if (Health != null) Health.Max = value; } }
        public Dice HPGrowth { get; set; } = new(1, 6);
        [NonTerm]
        public Stat Magic { get; set; }
        public int MP { get => Magic?.V ?? 0; set { if (Magic != null) Magic.V = value; } }
        public int MaxMP { get => Magic?.Max ?? 0; set { if (Magic != null) Magic.Max = value; } }
        public Dice MPGrowth { get; set; } = new(1, 6);
        public CorpseDef Corpse { get; set; }
        /// Enables delayed HP regen
        public int LastTookDamageOnTurn { get; set; } = int.MinValue;
        public int? LastAttackedBy { get; set; } = null;
    }
}
