using System.Collections.Generic;

namespace Fiero.Business
{
    public sealed class ActorStandings
    {
        private readonly Dictionary<int, StandingName> _dict;

        public void Set(Actor other, StandingName standing)
        {
            _dict[other.Id] = standing;
        }

        public bool TryGet(Actor other, out StandingName standing)
        {
            return _dict.TryGetValue(other.Id, out standing);
        }

        public ActorStandings()
        {
            _dict = new Dictionary<int, StandingName>();
        }
    }
}
