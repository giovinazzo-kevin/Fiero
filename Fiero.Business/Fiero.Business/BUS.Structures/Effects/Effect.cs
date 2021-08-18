using System;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{

    public abstract class Effect
    {
        protected readonly HashSet<Subscription> Subscriptions = new();

        public event Action<Effect> Started;
        public event Action<Effect> Ended;

        public abstract EffectName Name { get; }
        public abstract string DisplayName { get; }
        public abstract string DisplayDescription { get; }


        protected abstract IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner);
        protected virtual void OnStarted(GameSystems systems, Entity owner) { }
        public void Start(GameSystems systems, Entity owner)
        {
            Subscriptions.Add(systems.Action.GameStarted.SubscribeHandler(e => { End(); }));
            if(!(this is ModifierEffect)) {
                Started += e => owner.Effects?.Active.Add(e);
                Ended += e => owner.Effects?.Active.Remove(e);
                if (owner.TryCast<Actor>(out var actor)) {
                    Started += e => systems.Action.ActorGainedEffect.Raise(new(actor, this));
                    Ended += e => systems.Action.ActorLostEffect.Raise(new(actor, this));
                }
            }
            Subscriptions.UnionWith(RouteEvents(systems, owner));
            Started?.Invoke(this);
            OnStarted(systems, owner);
        }

        protected virtual void OnEnded() { }
        public void End()
        {
            OnEnded();
            foreach (var sub in Subscriptions) {
                sub.Dispose();
            }
            Subscriptions.Clear();
            Ended?.Invoke(this);
        }

        public override int GetHashCode() => this is ModifierEffect ? base.GetHashCode() : Name.GetHashCode();
        public override bool Equals(object obj) => this is ModifierEffect ? ReferenceEquals(obj, this) : obj is Effect e && e.Name == Name;
    }
}
