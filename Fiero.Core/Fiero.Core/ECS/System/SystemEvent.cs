using System;
using System.Threading.Tasks;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    public class SystemEvent<TSystem, TArgs>
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

        public Subscription SubscribeHandler(Action<TArgs> doStuff)
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSystem, TArgs>>(x => Name.Equals(x.Content.Sender))
                .Do<SystemMessage<TSystem, TArgs>>(msg => doStuff(msg.Content.Data))
                .Build()
                .Listen(Owner.EventHubName);
        }

        public Subscription SubscribeHandler(Action doStuff)
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSystem, TArgs>>(x => Name.Equals(x.Content.Sender))
                .Do<SystemMessage<TSystem, TArgs>>(_ => doStuff())
                .Build()
                .Listen(Owner.EventHubName);
        }
    }
}
