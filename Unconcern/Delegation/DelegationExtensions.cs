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
        public static void Fire(this IDelegateExpression expr, string hub)
        {
            if (expr.Triggers.Any())
                throw new InvalidOperationException("Cannot fire an expression that has triggers bound to it, call Listen() instead.");
            using (expr.Listen(hub)) { }
        }

        public static Subscription Listen(this IDelegateExpression expression, string hub = null)
        {
            var subs = new List<Subscription>();
            foreach (var expr in expression.Siblings.Append(expression)) {
                subs.Add(Subscribe(expr));
            }
            return new Subscription(subs);

            Subscription Subscribe(IDelegateExpression expression)
            {
                var subscriptions = new List<Subscription>();
                foreach (var when in expression.Triggers) {
                    var i = subscriptions.Count;
                    var sub = expression.Bus.Register(msg => {
                        if ((hub == null || msg.Recipients.Contains(hub) || msg.Recipients.Length == 0) && when(msg)) {
                            Fire(msg);
                        }
                    });
                    subscriptions.Add(sub);
                }
                if (!expression.Triggers.Any()) {
                    Fire(new EventBus.Message(DateTime.Now, typeof(object), null, hub, new[] { hub }));
                }
                return new Subscription(subscriptions, throwOnDoubleDispose: true);
            }

            void Fire(EventBus.Message msg = default)
            {
                foreach (var transform in expression.Replies) {
                    try {
                        var reply = transform(msg);
                        expression.Bus.Send(reply.FromHub(hub));
                    }
                    catch(InvalidCastException e) when (e.Message == "wrong_reply") {
                        ;
                    }
                }
                foreach (var handler in expression.Handlers) {
                    try {
                        handler(msg);
                    }
                    catch (InvalidCastException e) when (e.Message == "wrong_handler") {
                        ;
                    }
                }
            }
        }

        internal static IDelegateExpression WithCondition(this IDelegateExpression expr, Func<EventBus.Message, bool> cond) 
            => new DelegateExpression(expr.Bus, expr.Triggers.Append(cond), expr.Replies, expr.Handlers, expr.Siblings);

        internal static IDelegateExpression WithReply(this IDelegateExpression expr, Func<EventBus.Message, EventBus.Message> reply) 
            => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies.Append(reply), expr.Handlers, expr.Siblings);

        internal static IDelegateExpression WithHandler(this IDelegateExpression expr, Action<EventBus.Message> handler) => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies, expr.Handlers.Append(handler), expr.Siblings);

        internal static IDelegateExpression WithSibling(this IDelegateExpression expr, IDelegateExpression other) => new DelegateExpression(expr.Bus, expr.Triggers, expr.Replies, expr.Handlers, expr.Siblings.Append(other));

        public static bool IsFrom<T>(this EventBus.Message<T> msg, string hub) => msg.Sender == hub;
        public static bool HasRecipient<T>(this EventBus.Message<T> msg, string hub) => msg.Recipients.Contains(hub);
    }
}