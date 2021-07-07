using Fiero.Core;

namespace Fiero.Business
{
    public class Potion : Consumable
    {
        [RequiredComponent]
        public PotionComponent PotionProperties { get; private set; }
    }
}
