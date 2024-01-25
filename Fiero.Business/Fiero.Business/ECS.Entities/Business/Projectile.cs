using Fiero.Core;

namespace Fiero.Business
{
    public class Projectile : Consumable
    {
        [RequiredComponent]
        public ProjectileComponent ProjectileProperties { get; private set; }
    }
}
