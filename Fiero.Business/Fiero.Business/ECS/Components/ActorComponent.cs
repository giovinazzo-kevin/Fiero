using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class ActorComponent : Component
    {
        public int Health { get; set; }
        public int MaximumHealth { get; set; }

        public ActorName Type { get; set; }
        public Floor CurrentFloor { get; set; }
        public ActorRelationships Relationships { get; set; } = new();
        public Personality Personality { get; set; } = new();
        public List<Item> Inventory { get; set; } = new();
    }
}
