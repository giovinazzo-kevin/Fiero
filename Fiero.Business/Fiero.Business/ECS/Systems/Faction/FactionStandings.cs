using System;
using System.Collections.Generic;

namespace Fiero.Business
{

    public sealed class FactionStandings
    {
        public readonly FactionName Faction;
        private readonly Dictionary<FactionName, StandingName> _dict;

        public void Set(FactionName faction, StandingName standing)
        {
            if (faction == Faction)
                throw new ArgumentException("Can't set faction standing of self");
            _dict[faction] = standing;
        }

        public StandingName Get(FactionName faction)
        {
            if (faction == Faction)
                return StandingName.Loved;
            return _dict[faction];
        }

        public FactionStandings(FactionName faction)
        {
            Faction = faction;
            _dict = new Dictionary<FactionName, StandingName>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                if (val == Faction)
                    continue;
                _dict[val] = StandingName.Neutral;
            }
        }
    }
}
