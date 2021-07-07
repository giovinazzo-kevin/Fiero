using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class ActorComponent : Component
    {
        public int Health { get; set; } = 1;
        public int MaximumHealth { get; set; } = 1;

        public ActorName Type { get; set; }
        public MonsterTierName Tier { get; set; }
        public Floor CurrentFloor { get; set; }
        public ActorRelationships Relationships { get; set; } = new();
        public Personality Personality { get; set; } = new();
    }
}
