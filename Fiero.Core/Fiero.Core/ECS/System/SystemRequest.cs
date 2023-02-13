using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    public interface ISystemRequest : ISystemEvent
    {
        Subscription SubscribeResponse(Func<object, object> transform);
    }

    public class SystemRequest<TSys, TArgs, TResponseArgs>
        : SystemEvent<TSys, TArgs>, ISystemRequest
        where TSys : EcsSystem
    {
        public event Action<SystemRequest<TSys, TArgs, TResponseArgs>, TArgs, IEnumerable<TResponseArgs>> ResponseReceived;

        public SystemRequest(TSys owner, string name)
            : base(owner, name)
        {
        }

        public IEnumerable<TResponseArgs> Request(TArgs args)
        {
            using var sieve = new Sieve<TResponseArgs>(Owner.EventBus, msg => msg.IsFrom(Name));
            Raise(args);
            var messages = sieve.Messages.Select(x => x.Content).ToList();
            ResponseReceived?.Invoke(this, args, messages);
            return messages;
        }

        Subscription ISystemRequest.SubscribeResponse(Func<object, object> transform)
        {
            return SubscribeResponse(arg => (TResponseArgs)transform(arg));
        }

        public Subscription SubscribeResponse(Func<TArgs, TResponseArgs> transform)
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSys, TArgs>>
                    (msg => Name.Equals(msg.Content.Sender))
                .Reply<SystemMessage<TSys, TArgs>, TResponseArgs>
                    (msg =>
                    {
                        var data = transform(msg.Content.Data);
                        var ret = msg.WithContent(data).From(Name).To(msg.Sender);
                        return ret;
                    })
                .Build()
                .Listen(Owner.EventHubName);
        }
    }
}
