using Fiero.Core.Exceptions;
using Unconcern.Common;

namespace Fiero.Core
{
    /// <summary>
    /// Handles caching of scripts and their top-level routing through a provided IScriptHost.
    /// </summary>
    public class GameScripts(IServiceFactory fac, MetaSystem meta, GameDataStore store)
    {
        protected readonly Dictionary<string, Script> Scripts = new();
        public readonly Dictionary<Type, IScriptHost> Hosts = fac.GetAllInstances<IScriptHost>()
            .SelectMany(x => x.GetType()
                .GetInterfaces()
                .Select(y => (Host: x, Interface: y)))
                .Where(z => z.Interface.IsGenericType && z.Interface.GetGenericTypeDefinition() == typeof(IScriptHost<>))
                .DistinctBy(x => x.Interface)
                .Select(w => (w.Host, Type: w.Interface.GetGenericArguments()[0]))
            .ToDictionary(x => x.Type, x => x.Host);

        private ScriptDataRoutes DataRoutes;
        private ScriptEventRoutes EventRoutes;
        private Subscription Unsub = new(true);

        private bool TryGetScriptHost(Type t, out IScriptHost host) => Hosts.TryGetValue(t, out host);
        public bool TryGetScriptHost<T>(out IScriptHost<T> host) where T : Script
        {
            if (Hosts.TryGetValue(typeof(T), out var host_))
            {
                host = (IScriptHost<T>)host_;
                return true;
            }
            host = default;
            return false;
        }

        private void Run(Script script)
        {
            if (!TryGetScriptHost(script.GetType(), out var host))
                throw new NotSupportedException(script.GetType().Name);
            Unsub.Add([script.Run(EventRoutes ??= host.GetScriptEventRoutes(meta), DataRoutes ??= host.GetScriptDataRoutes(store))]);
        }

        public bool TryLoad<T>(string key, out T script)
            where T : Script
        {
            if (!TryGetScriptHost<T>(out var host))
                throw new NotSupportedException(typeof(T).Name);
            if (host.TryLoad(key, out script))
            {
                Scripts[key] = script;
                Run(script);
                return true;
            }
            return false;
        }
        public bool TryGet<T>(string key, out T script)
            where T : Script
        {
            script = default;
            if (!Scripts.TryGetValue(key, out var script_))
            {
                if (TryLoad(key, out script))
                    return true;
                return false;
            }
            script = (T)script_;
            return true;
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
            // This is because scripts are loaded before that happens, so they're already running once we get here.
            // That's okay, because routes are available pretty much as soon as all systems are created.
            foreach (var script in Scripts.Values.Where(s => !s.IsRunning))
                Run(script);
            // When unsubbing we don't outright unload the scripts, as loading them is fairly expensive,
            // and some scripts are (pre)loaded only once when the game initializes.
            // Instead we simply dispose their subscriptions which is equivalent to stopping their execution.
            // Then once the game restarts we can re-route any cached script that isn't running anymore.
            // In case a script wasn't loaded yet, then it will be routed as usual once it is.
            // TODO: Add a hook that is called when a script stops executing, so that it can clean up any dynamic predicates it set up.
            yield return Unsub + new Subscription(new Action[] { () => {
                Unsub = new(true);
            } }, true);
        }
    }
}
