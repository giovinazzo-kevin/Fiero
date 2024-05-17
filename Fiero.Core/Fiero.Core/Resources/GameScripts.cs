using Fiero.Core.Exceptions;
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

        private ScriptDataRoutes DataRoutes;
        private ScriptEventRoutes EventRoutes;
        private Subscription Unsub = new(true);

        private void Run(Script script)
        {
            Unsub.Add([script.Run(EventRoutes ??= Host.GetScriptEventRoutes(meta), DataRoutes ??= Host.GetScriptDataRoutes(store))]);
        }

        public bool TryLoad(string key, out Script script)
        {
            if (Host.TryLoad(key, out script))
            {
                Scripts[key] = script;
                Run(script);
                return true;
            }
            return false;
        }
        public bool TryGet(string key, out Script script) => Scripts.TryGetValue(key, out script);
        public bool TryGet<T>(string key, out T script)
            where T : Script
        {
            script = default;
            if (!TryGet(key, out var script_) && !TryLoad(key, out script_))
            {
                return false;
            }
            if (script_ is T t)
            {
                script = t;
                return true;
            }
            return false;
        }
        public T Get<T>(string key)
            where T : Script
        {
            if (TryGet(key, out T script))
                return script;
            throw new ScriptNotFoundException(key);
        }

        public IEnumerable<Subscription> RouteSubscriptions()
        {
            yield return Unsub + new Subscription(new Action[] { () => {
                Unsub = new(true);
                foreach (var script in Scripts.Values)
                    Run(script);
            } }, true);
        }
    }
}
