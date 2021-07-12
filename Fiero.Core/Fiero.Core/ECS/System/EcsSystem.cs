using System;
using System.Reflection;
using Unconcern.Common;

namespace Fiero.Core
{

    [SingletonDependency]
    public abstract class EcsSystem : IDisposable
    {
        public readonly EventBus EventBus;
        public virtual string EventHubName => GetType().Name;

        public event Action<EcsSystem> Disposing;

        public EcsSystem(EventBus bus)
        {
            EventBus = bus;
        }

        public virtual void Dispose()
        {
            Disposing?.Invoke(this);
        }
    }
}
