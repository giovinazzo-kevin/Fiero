using System;

namespace Fiero.Business
{
    internal readonly struct ActorTime
    {
        public readonly int ActorId;
        public readonly int Time;
        public readonly Func<int?> Act;

        public ActorTime(int actorId, Func<int?> actionCost, int time = 0)
        {
            Time = time;
            ActorId = actorId;
            Act = actionCost;
        }

        public ActorTime WithTime(int newTime) => new(ActorId, Act, newTime);
    }
}
