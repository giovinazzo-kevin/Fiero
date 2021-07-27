using Fiero.Core;

namespace Fiero.Business
{
    public class BloodSplatter : Feature
    {
        [RequiredComponent]
        public BloodComponent Blood { get; private set; }
    }
}
