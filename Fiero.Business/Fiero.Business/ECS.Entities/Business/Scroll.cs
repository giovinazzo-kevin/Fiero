using Fiero.Core;

namespace Fiero.Business
{
    public class Scroll : Projectile
    {
        [RequiredComponent]
        public ScrollComponent ScrollProperties { get; private set; }
    }
}
