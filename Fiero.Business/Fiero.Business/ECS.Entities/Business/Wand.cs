using Fiero.Core;

namespace Fiero.Business
{
    public class Wand : Projectile
    {
        [RequiredComponent]
        public WandComponent WandProperties { get; private set; }
    }
}
