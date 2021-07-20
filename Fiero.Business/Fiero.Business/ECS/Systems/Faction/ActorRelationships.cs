using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public sealed class ActorRelationships
    {
        private readonly Dictionary<int, Relationship> _dict;

        public void Set(Actor other, Relationship standing)
        {
            _dict[other.Id] = standing;
        }

        public void Update(Actor other, Func<Relationship, Relationship> update, out Relationship value)
        {
            if(!TryGet(other, out value)) {
                value = new(StandingName.Tolerated);
            }
            _dict[other.Id] = value = update(value);
        }

        public bool TryGet(Actor other, out Relationship standing)
        {
            return _dict.TryGetValue(other.Id, out standing);
        }

        public ActorRelationships()
        {
            _dict = new Dictionary<int, Relationship>();
        }
    }
}
