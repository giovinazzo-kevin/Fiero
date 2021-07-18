using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    public class SystemRequest<TSys, TArgs, TResponseArgs>
        : SystemEvent<TSys, TArgs>
        where TSys : EcsSystem
    {
        public SystemRequest(TSys owner, string name)
            : base(owner, name)
        {
        }

        public IEnumerable<TResponseArgs> Request(TArgs args)
        {
            using var sieve = new Sieve<TResponseArgs>(Owner.EventBus, msg => msg.IsFrom(Name));
            Raise(args);
            foreach (var response in sieve.Messages) {
                yield return response.Content;
            }
        }

        public Subscription SubscribeResponse(Func<TArgs, TResponseArgs> transform)
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSys, TArgs>>
                    (msg => Name.Equals(msg.Content.Sender))
                .Reply<SystemMessage<TSys, TArgs>, TResponseArgs>
                    (msg => {
                        var data = transform(msg.Content.Data);
                        var ret = msg.WithContent(data).From(Name).To(msg.Sender);
                        return ret;
                    })
                .Build()
                .Listen(Owner.EventHubName);
        }
    }
}
