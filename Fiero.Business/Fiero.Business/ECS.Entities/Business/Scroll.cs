using Fiero.Core;

namespace Fiero.Business
{
    public class Scroll : Consumable
    {
        [RequiredComponent]
        public ScrollComponent ScrollProperties { get; private set; }
    }
}
