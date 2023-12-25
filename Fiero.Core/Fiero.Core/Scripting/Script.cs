using Unconcern.Common;

namespace Fiero.Core
{
    public abstract class Script
    {
        public readonly record struct EventHook(string System, string Event);
        public readonly record struct DataHook(string Name);
        public abstract IEnumerable<EventHook> EventHooks { get; }
        public abstract IEnumerable<DataHook> DataHooks { get; }
        public abstract Subscription Run(ScriptEventRoutes eventRoutes, ScriptDataRoutes dataRoutes);


    }
}
