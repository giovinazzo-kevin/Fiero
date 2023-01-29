using System;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    public interface ISystemEvent
    {
        Subscription SubscribeHandler(Action<object> handle);
    }
    public class SystemEvent<TSystem, TArgs> : ISystemEvent
        where TSystem : EcsSystem
    {
        public readonly string Name;
        public readonly TSystem Owner;

        public SystemEvent(TSystem owner, string name)
        {
            Name = name;
            Owner = owner;
        }

        public void Raise(TArgs args)
        {
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
