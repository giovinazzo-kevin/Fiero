using Unconcern.Common;

namespace Fiero.Core
{

    [SingletonDependency]
    public abstract class EcsSystem : IDisposable
    {
        public readonly record struct SystemCreatedEvent(EcsSystem System);
        public readonly record struct SystemDisposedEvent(EcsSystem System);
        public readonly SystemEvent<EcsSystem, SystemCreatedEvent> Created;
        public readonly SystemEvent<EcsSystem, SystemCreatedEvent> Disposed;


        public readonly EventBus EventBus;
        public virtual string EventHubName => GetType().Name;

        protected readonly List<Subscription> Subscriptions = new();

        public EcsSystem(EventBus bus)
        {
            EventBus = bus;
            Created = new(this, nameof(Created));
            Disposed = new(this, nameof(Disposed));
            _ = Created.Raise(new(this));
        }

        public virtual void Dispose()
        {
            _ = Disposed.Raise(new(this));
            foreach (var sub in Subscriptions)
                sub.Dispose();
        }
    }
}
