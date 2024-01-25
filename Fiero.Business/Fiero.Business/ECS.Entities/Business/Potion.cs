using Fiero.Core;

namespace Fiero.Business
{
    public class Potion : Projectile
    {
        [RequiredComponent]
        public PotionComponent PotionProperties { get; private set; }
    }
}
