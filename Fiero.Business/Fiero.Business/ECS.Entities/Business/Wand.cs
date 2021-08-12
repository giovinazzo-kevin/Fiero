using Fiero.Core;

namespace Fiero.Business
{
    public class Wand : Consumable
    {
        [RequiredComponent]
        public WandComponent WandProperties { get; private set; }
    }
}
