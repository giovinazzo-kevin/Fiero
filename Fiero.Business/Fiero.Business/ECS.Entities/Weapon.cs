using Fiero.Core;

namespace Fiero.Business
{
    public class Weapon : Item
    {
        [RequiredComponent]
        public WeaponComponent WeaponProperties { get; private set; }
    }
}
