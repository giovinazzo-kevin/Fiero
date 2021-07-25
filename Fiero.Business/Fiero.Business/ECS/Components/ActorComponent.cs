using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class ActorComponent : EcsComponent
    {
        public ActorName Type { get; set; }
        public MonsterTierName Tier { get; set; }
        public ActorStats Stats { get; set; } = new();
        public ActorRelationships Relationships { get; set; } = new();
    }
}
