using Fiero.Core;

namespace Fiero.Business
{
    public class Weapon : Equipment
    {
        [RequiredComponent]
        public WeaponComponent WeaponProperties { get; private set; }
    }
}
