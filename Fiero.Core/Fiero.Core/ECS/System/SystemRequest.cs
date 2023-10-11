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
        public event Action<SystemRequest<TSys, TArgs, TResponseArgs>, TArgs, TResponseArgs> ResponsesReceived;
        public event Action<SystemRequest<TSys, TArgs, TResponseArgs>, TArgs, IEnumerable<TResponseArgs>> AllResponsesReceived;

        public SystemRequest(TSys owner, string name, bool asynchronous = false)
            : base(owner, name, asynchronous)
        {
        }

        public async IAsyncEnumerable<TResponseArgs> Request(TArgs args)
        {
            using var sieve = new Sieve<TResponseArgs>(Owner.EventBus, msg => msg.IsFrom(Name));
            if (Asynchronous)
            {
                var cts = new CancellationTokenSource();
                var responses = new List<TResponseArgs>();
                _ = Task.Run(async () =>
                {
                    await Raise(args);
                    cts.Cancel();
                });
                await foreach (var msg in sieve.EnumerateAsync(cts.Token))
                {
                    ResponsesReceived?.Invoke(this, args, msg.Content);
                    responses.Add(msg.Content);
                    yield return msg.Content;
                }
                AllResponsesReceived?.Invoke(this, args, responses);
            }
            else
            {
                _ = Raise(args);
                var responses = sieve.Messages.Select(x => x.Content).ToArray();
                foreach (var msg in responses)
                {
                    ResponsesReceived?.Invoke(this, args, msg);
                    yield return msg;
                }
                AllResponsesReceived?.Invoke(this, args, responses);
            }
        }

        Subscription ISystemRequest.SubscribeResponse(Func<object, object> transform)
        {
            return SubscribeResponse(arg => (TResponseArgs)transform(arg));
        }

        public Subscription SubscribeResponse(Func<TArgs, TResponseArgs> transform)
        {
            var expr = Concern.Delegate(Owner.EventBus)
                .When<SystemMessage<TSys, TArgs>>
                    (msg => Name.Equals(msg.Content.Sender));
            if (Asynchronous)
            {
                expr = expr.PostReply<SystemMessage<TSys, TArgs>, TResponseArgs>(Reply);
            }
            else
            {
                expr = expr.SendReply<SystemMessage<TSys, TArgs>, TResponseArgs>(Reply);
            }
            Message<TResponseArgs> Reply(Message<SystemMessage<TSys, TArgs>> msg)
            {
                var data = transform(msg.Content.Data);
                var ret = msg.WithContent(data).From(Name).To(msg.Sender);
                return ret;
            }

            return expr
                .Build()
                .Listen(Owner.EventHubName);
        }
    }
}
