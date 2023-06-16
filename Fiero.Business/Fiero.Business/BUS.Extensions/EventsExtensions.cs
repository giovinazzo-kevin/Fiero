using Fiero.Core;
using System;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public static class EventsExtensions
    {
        public static bool Handle<TSys, TArgs>(this SystemRequest<TSys, TArgs, EventResult> req, TArgs payload)
            where TSys : EcsSystem
        {
            return req.Request(payload).All(x => x);
        }

        public static void HandleOrThrow<TSys, TArgs>(this SystemRequest<TSys, TArgs, EventResult> req, TArgs payload)
            where TSys : EcsSystem
        {
            if (!req.Handle(payload))
                throw new InvalidOperationException();
        }

        public static void SubscribeUntil<TSys, TArgs>(this SystemEvent<TSys, TArgs> evt, Func<TArgs, bool> until)
            where TSys : EcsSystem
        {
            var sub = default(Subscription);
            sub = evt.SubscribeHandler(msg =>
            {
                if (until(msg))
                {
                    sub.Dispose();
                }
            });
        }
    }
}
