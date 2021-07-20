using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public sealed class FactionRelationships
    {
        public readonly FactionName Faction;
        private readonly Dictionary<FactionName, Relationship> _dict;

        public void Set(FactionName faction, Relationship standing)
        {
            if (faction == Faction)
                throw new ArgumentException("Can't set faction standing of self");
            _dict[faction] = standing;
        }

        public void Update(FactionName other, Func<Relationship, Relationship> update, out Relationship value)
        {
            _dict[other] = value = update(_dict[other]);
        }

        public Relationship Get(FactionName faction)
        {
            if (faction == Faction)
                return new(StandingName.Loved);
            return _dict[faction];
        }

        public FactionRelationships(FactionName faction)
        {
            Faction = faction;
            _dict = new Dictionary<FactionName, Relationship>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                if (val == Faction)
                    continue;
                _dict[val] = new(StandingName.Tolerated);
            }
        }
    }
}
