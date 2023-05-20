using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class PartyComponent : EcsComponent
    {
        public Actor Leader { get; set; }
        public List<Actor> Followers { get; set; } = new();
    }
}
