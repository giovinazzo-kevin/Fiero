using System;

namespace Fiero.Business
{
    internal readonly struct ActorTime
    {
        public readonly int ActorId;
        public readonly Actor Proxy;
        public readonly int Time;
        public readonly Func<IAction> GetIntent;

        public ActorTime(int actorId, Actor proxy, Func<IAction> actionCost, int time = 0)
        {
            Time = time;
            Proxy = proxy;
            ActorId = actorId;
            GetIntent = actionCost;
        }

        public ActorTime WithTime(int newTime) => new(ActorId, Proxy, GetIntent, newTime);
    }
}
