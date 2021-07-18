using System;

namespace Fiero.Business
{
    internal readonly struct ActorTime
    {
        public readonly int Time;
        public readonly int ActorId;
        public readonly Actor Actor;
        public readonly Func<IAction> GetIntent;

        public ActorTime(int actorId, Actor proxy, Func<IAction> actionCost, int time = 0)
        {
            Time = time;
            Actor = proxy;
            ActorId = actorId;
            GetIntent = actionCost;
        }

        public ActorTime WithTime(int newTime) => new(ActorId, Actor, GetIntent, newTime);
    }
}
