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

        public bool TryUpdate(Actor other, Func<Relationship, Relationship> update, out Relationship value)
        {
            if (_dict.TryGetValue(other.Id, out value)) {
                value = _dict[other.Id] = update(value);
                return true;
            }
            return false;
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
