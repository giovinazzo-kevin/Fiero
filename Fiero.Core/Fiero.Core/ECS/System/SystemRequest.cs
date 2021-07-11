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
        public readonly SystemEvent<TSys, TResponseArgs> Response;

        public SystemRequest(TSys owner, string name)
            : base(owner, name)
        {
            Response = new SystemEvent<TSys, TResponseArgs>(owner, $"Response({name})");
        }

        public async IAsyncEnumerable<TResponseArgs> Send(TArgs args)
        {
            await using var sieve = new Sieve<SystemMessage<TSys, TResponseArgs>>(Owner.EventBus, msg => true);
            Raise(args);
            foreach (var response in sieve.Responses) {
                yield return response.Content.Message;
            }
        }

        public Task<Subscription> SubscribeResponse<TOtherSystem>(Func<SystemMessage<TSys, TArgs>, SystemMessage<TOtherSystem, TResponseArgs>> transform)
            where TOtherSystem : EcsSystem
        {
            return Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSys, TArgs>>
                    (msg => Name.Equals(msg.Content.Sender))
                .Reply<SystemMessage<TSys, TArgs>, SystemMessage<TOtherSystem, TResponseArgs>>
                    (msg => {
                        var response = msg.WithContent(transform(msg.Content));
                        Response.Raise(response.Content.Message);
                        return response;
                    })
                .Build()
                .Listen(Owner.EventHubName);
        }
    }
}
