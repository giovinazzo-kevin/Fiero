using Fiero.Core;

namespace Fiero.Business
{
    public class Wand : Throwable
    {
        [RequiredComponent]
        public WandComponent WandProperties { get; private set; }
    }
}
