using Fiero.Core;

namespace Fiero.Business
{
    public class WeaponComponent : EcsComponent
    {
        public WeaponName Type { get; set; }
        public WeaponHandednessName Handedness { get; set; }
        public int BaseDamage { get; set; }
        public int SwingDelay { get; set; }
    }
}
