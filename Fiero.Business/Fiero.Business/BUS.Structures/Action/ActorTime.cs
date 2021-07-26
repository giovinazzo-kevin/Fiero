using System;

namespace Fiero.Business
{
    public readonly struct ActorTime
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

        public override int GetHashCode() => ActorId;
        public override bool Equals(object obj) => obj is ActorTime t && t.ActorId == ActorId;

        public static bool operator ==(ActorTime left, ActorTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorTime left, ActorTime right)
        {
            return !(left == right);
        }
    }
}
