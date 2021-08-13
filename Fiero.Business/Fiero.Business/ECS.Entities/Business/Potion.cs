using Fiero.Core;

namespace Fiero.Business
{
    public class Potion : Throwable
    {
        [RequiredComponent]
        public PotionComponent PotionProperties { get; private set; }
    }
}
