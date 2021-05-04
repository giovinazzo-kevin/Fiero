using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class FactionSystem
    {
        protected readonly Dictionary<FactionName, FactionStandings> Standings;

        public FactionStandings GetStandings(FactionName faction) => Standings[faction];
        public void SetMutualStanding(FactionName a, FactionName b, StandingName aTowardsB, StandingName? bTowardsA = null)
        {
            GetStandings(a).Set(b, aTowardsB);
            GetStandings(b).Set(a, bTowardsA ?? aTowardsB);
        }

        public FactionSystem()
        {
            Standings = new Dictionary<FactionName, FactionStandings>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                Standings[val] = new FactionStandings(val);
            }
        }
    }
}
