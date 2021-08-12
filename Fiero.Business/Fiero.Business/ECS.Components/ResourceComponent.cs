using Fiero.Core;

namespace Fiero.Business
{
    public class ResourceComponent : EcsComponent
    {
        public int Amount { get; set; }
        public int MaximumAmount { get; set; }
        public ResourceName Name { get; set; }
    }
}
