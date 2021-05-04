using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{

    public class ActorComponent : Component
    {
        public int Health { get; set; }
        public int MaximumHealth { get; set; }
        public bool IsBoss => Type == ActorName.GreatKingRat;

        public ActorName Type { get; set; }
        public Floor CurrentFloor { get; set; }
        public ActorStandings Standings { get; set; } = new ActorStandings();
        public List<Item> Inventory { get; set; } = new List<Item>();
    }
}
