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

        protected abstract IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner);
        protected virtual void OnStarted(MetaSystem systems, Entity owner, Entity source)
        {

        }
        public void Start(MetaSystem systems, Entity owner, Entity source)
        {
            if (owner?.Effects?.Lock ?? false)
                return;
            var action = systems.Get<ActionSystem>();
            Subscriptions.Add(action.GameStarted.SubscribeHandler(e => { End(systems, owner); }));
            if (!(this is ModifierEffect))
            {
                Started += e => owner.Effects?.Active.Add(e);
                Ended += e => owner.Effects?.Active.Remove(e);
                if (owner.TryCast<Actor>(out var actor))
                {
                    Started += e => _ = action.ActorGainedEffect.Raise(new(actor, this));
                    Ended += e => _ = action.ActorLostEffect.Raise(new(actor, this));
                }
            }
            Subscriptions.UnionWith(RouteEvents(systems, owner));
            Started?.Invoke(this);
            OnStarted(systems, owner, source);
        }

        protected virtual void OnEnded(MetaSystem systems, Entity owner) { }
        public void End(MetaSystem systems, Entity owner)
        {
            foreach (var sub in Subscriptions)
            {
                sub.Dispose();
            }
            Subscriptions.Clear();
            Ended?.Invoke(this);
            OnEnded(systems, owner);
        }

        public override int GetHashCode() => this is ModifierEffect ? base.GetHashCode() : Name.GetHashCode();
        public override bool Equals(object obj) => this is ModifierEffect ? ReferenceEquals(obj, this) : obj is Effect e && e.Name == Name;
    }
}
