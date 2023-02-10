using Fiero.Core;

namespace Fiero.Business
{
    public class ActorComponent : EcsComponent
    {
        public ActorName Type { get; set; }
        public Stat Health { get; set; }
        public CorpseDef Corpse { get; set; }
    }
}
