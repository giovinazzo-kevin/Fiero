using Unconcern.Common;

namespace Fiero.Core
{
    /// <summary>
    /// Handles caching of scripts and their top-level routing through a provided IScriptHost.
    /// </summary>
    public class GameScripts(IScriptHost host, MetaSystem meta, GameDataStore store)
    {
        protected readonly Dictionary<string, Script> Scripts = new();
        public readonly IScriptHost Host = host;

        public bool TryLoad(string key, out Script script)
        {
            if (Host.TryLoad(key, out script))
            {
                Scripts[key] = script;
                return true;
            }
            return false;
        }
        public bool TryGet(string key, out Script script) => Scripts.TryGetValue(key, out script);
        public bool TryGet<T>(string key, out T script)
            where T : Script
        {
            if (TryGet(key, out var script_) && script_ is T t)
            {
                script = t;
                return true;
            }
            script = default;
            return false;
        }
        public Script Get(string key) => Scripts[key];

        public IEnumerable<Subscription> RouteSubscriptions()
        {
            var eventRoutes = Host.GetScriptEventRoutes(meta);
            var dataRoutes = Host.GetScriptDataRoutes(store);
            foreach (var item in Scripts.Values)
                yield return item.Run(eventRoutes, dataRoutes);
        }
    }
}
