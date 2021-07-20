using System;

namespace Fiero.Business
{
    internal readonly struct ActorTime
    {
        public readonly int LastActedTime;
        public readonly int Time;
        public readonly int ActorId;
        public readonly Actor Actor;
        public readonly Func<IAction> GetIntent;

        public ActorTime(int actorId, Actor proxy, Func<IAction> actionCost, int time = 0, int last = 0)
        {
            Time = time;
            Actor = proxy;
            ActorId = actorId;
            GetIntent = actionCost;
            LastActedTime = last;
        }

        public ActorTime WithTime(int newTime) => new(ActorId, Actor, GetIntent, newTime, LastActedTime);
        public ActorTime WithLastActedTime(int newLast) => new(ActorId, Actor, GetIntent, Time, newLast);
    }
}
