using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public static class DelegationExtensions
    {
        public static async Task Fire(this IDelegateExpression expr, string hub, CancellationToken ct = default)
        {
            if (expr.Triggers.Any())
                throw new InvalidOperationException("Cannot fire an expression that has triggers bound to it, call Listen() instead.");
            await using (await expr.Listen(hub, ct)) { }
        }

        public static async Task<Subscription> Listen(this IDelegateExpression expression, string hub = null, CancellationToken ct = default)
        {
            var subs = new List<Subscription>();
            foreach (var expr in expression.Siblings.Append(expression)) {
                subs.Add(await Subscribe(expr, ct));
            }
            return new Subscription(subs);

            async Task<Subscription> Subscribe(IDelegateExpression expression, CancellationToken ct = default)
            {
                var subscriptions = new List<Subscription>();
                foreach (var when in expression.Triggers) {
                    var i = subscriptions.Count;
                    var sub = expression.Bus.Register(async msg => {
                        if ((hub == null || msg.Recipients.Contains(hub) || msg.Recipients.Length == 0) && await when(msg)) {
                            await Fire(msg, ct);
                        }
                    });
                    subscriptions.Add(sub);
                }
                if (!expression.Triggers.Any()) {
                    await Fire(new EventBus.Message(DateTime.Now, typeof(object), null, hub, new[] { hub }), ct);
                }
                return new Subscription(subscriptions, throwOnDoubleDispose: true);
            }

            async Task Fire(EventBus.Message msg = default, CancellationToken ct = default)
            {
                foreach (var transform in expression.Replies) {
                    try {
                        var reply = await transform(msg);
                        expression.Bus.Send(reply.FromHub(hub));
                    }
                    catch(InvalidCastException e) when (e.Message == "wrong_reply") {
                        ;
                    }
                }
                foreach (var handler in expression.Handlers) {
                    if (ct.IsCancellationRequested) {
                        return;
                    }
                    try {
                        await handler(msg);
                    }
                    catch (InvalidCastException e) when (e.Message == "wrong_handler") {
                        ;
                    }
                }
            }
        }

        internal static IDelegateExpression WithCondition(this IDelegateExpression expr, Func<EventBus.Message, Task<bool>> cond) 
            => new DelegateExpression(expr.Bus, expr.Triggers.Append(cond), expr.Replies, expr.Handlers, expr.Siblings);

        internal static IDelegateExpression WithReply(this IDelegateExpression expr, Func<EventBus.Message, Task<EventBus.Message>> reply) 
            => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies.Append(reply), expr.Handlers, expr.Siblings);

        internal static IDelegateExpression WithHandler(this IDelegateExpression expr, Func<EventBus.Message, Task> handler) => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies, expr.Handlers.Append(handler), expr.Siblings);

        internal static IDelegateExpression WithSibling(this IDelegateExpression expr, IDelegateExpression other) => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies, expr.Handlers, expr.Siblings.Append(other));

        public static bool IsFrom<T>(this EventBus.Message<T> msg, string hub) => msg.Sender == hub;
        public static bool HasRecipient<T>(this EventBus.Message<T> msg, string hub) => msg.Recipients.Contains(hub);
    }
}