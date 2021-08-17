using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class ActorComponent : EcsComponent
    {
        public ActorName Type { get; set; }
        public Stat Health { get; set; }
    }
}
