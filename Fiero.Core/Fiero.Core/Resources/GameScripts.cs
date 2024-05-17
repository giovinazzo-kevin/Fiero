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
            // This foreach is entered from the *second* time RouteSubscriptions is called.
            // This is because scripts are loaded before that happens.
            // So they're already running once we get here.
            // When unsubbing we don't outright unload the scripts, as loading them is fairly expensive,
            // and some scripts are (pre)loaded only once when the game initializes.
            // Instead we simply dispose their subscriptions which is equivalent to stopping their execution.
            // Then once the game restarts we can re-route any cached script that isn't running anymore.

            // TODO: Add a hook that is called when a script stops executing, so that it can clean up any dynamic predicates it set up.

            foreach (var script in Scripts.Values.Where(s => !s.IsRunning))
                Run(script);
            yield return Unsub + new Subscription(new Action[] { () => {
                Unsub = new(true);
            } }, true);
        }
    }
}
