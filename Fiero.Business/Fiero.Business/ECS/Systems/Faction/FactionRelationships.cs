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
        public bool TryUpdate(FactionName faction, Func<Relationship, Relationship> update, out Relationship value)
        {
            if (_dict.TryGetValue(faction, out value)) {
                value = _dict[faction] = update(value);
                return true;
            }
            return false;
        }

        public Relationship Get(FactionName faction)
        {
            if (faction == Faction)
                return new(StandingName.Loved, TrustName.Admired, PowerComparisonName.FairFight);
            return _dict[faction];
        }

        public FactionRelationships(FactionName faction)
        {
            Faction = faction;
            _dict = new Dictionary<FactionName, Relationship>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                if (val == Faction)
                    continue;
                _dict[val] = new(StandingName.Tolerated, TrustName.Known, PowerComparisonName.FairFight);
            }
        }
    }
}
