using Unconcern.Common;

namespace Fiero.Core
{
    public abstract class Script
    {
        public readonly record struct EventHook(string System, string Event);

        public abstract IEnumerable<EventHook> Hooks { get; }
        public abstract Subscription Run(ScriptRoutes routes);


    }
}
