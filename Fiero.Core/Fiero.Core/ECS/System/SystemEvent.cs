using Ergo.Lang;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    public interface ISystemEvent
    {
        TermMarshallingContext MarshallingContext { get; }
        Subscription SubscribeHandler(Action<object> handle);
    }
    public class SystemEvent<TSystem, TArgs> : ISystemEvent
        where TSystem : EcsSystem
    {
        public readonly string Name;
        public readonly TSystem Owner;
        public readonly bool Asynchronous;
        public TermMarshallingContext MarshallingContext { get; private set; }

        public SystemEvent(TSystem owner, string name, bool asynchronous = false)
        {
            Name = name;
            Owner = owner;
            Asynchronous = asynchronous;
        }

        public async ValueTask Raise(TArgs args, CancellationToken ct = default)
        {
            // Ensure that all handlers use the same cache when calling ToTerm
            MarshallingContext = new();
            if (Asynchronous)
                await Owner.EventBus.Post(new SystemMessage<TSystem, TArgs>(Name, Owner, args), Owner.EventHubName, ct: ct);
            else
                Owner.EventBus.Send(new SystemMessage<TSystem, TArgs>(Name, Owner, args), Owner.EventHubName);
        }

        public Subscription SubscribeHandler(Action<TArgs> handle)
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSystem, TArgs>>(x => Name.Equals(x.Content.Sender))
                .Do<SystemMessage<TSystem, TArgs>>(msg => handle(msg.Content.Data))
                .Build()
                .Listen(Owner.EventHubName);
        }

        Subscription ISystemEvent.SubscribeHandler(Action<object> handle) => SubscribeHandler(x => handle(x));
    }
}
