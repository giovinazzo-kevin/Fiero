using Fiero.Core;

namespace Fiero.Business
{
    public class Resource : Item
    {
        [RequiredComponent]
        public ResourceComponent ResourceProperties { get; private set; }
    }
}
