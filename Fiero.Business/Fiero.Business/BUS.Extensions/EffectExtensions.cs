using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{
    public static class EventsExtensions
    {
        public static bool Handle<TSys, TData>(this SystemRequest<TSys, TData, EventResult> req, TData payload)
            where TSys : EcsSystem
        {
            return req.Request(payload).All(x => x);
        }
    }

    public static class EffectExtensions
    {
        public static TemporaryEffect Temporary(this Effect e, int duration)
        {
            return new(e, duration);
        }

        public static GrantOnUseEffect GrantedOnUse(this Effect e)
        {
            return new(e);
        }
    }
}
